using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Golfbutsmaller
{
    /**
     * Defines AI difficulty levels for the game
     */
    public enum AIDifficulty { Easy, Hard }

    /**
     * Manages the AI opponent's behaviour and shot calculations
     * Controls pathfinding, optimisation, and difficulty-based accuracy
     */
    public class AIPlayer
    {
        // Core difficulty settings
        private Random random = new Random();
        private AIDifficulty difficulty;
        private const float MAX_POWER = 30f;

        // Accuracy parameters for different difficulty levels
        private const float HARD_ACCURACY = 0.98f;  // Higher precision
        private const float EASY_ACCURACY = 0.70f;  // Lower precision 
        private const float HARD_POWER_CONTROL = 0.99f;
        private const float HARD_MIN_POWER = 0.8f;

        // Pathfinding configuration values
        private const float NODE_SPACING = 40f;
        private const float GOAL_THRESHOLD = 20f;

        // Current game state references
        private List<RotatingObstacle> _currentObstacles;
        private Level _currentLevel;

        public AIPlayer(AIDifficulty difficulty)
        {
            this.difficulty = difficulty;
        }

        public AIDifficulty GetDifficulty() => difficulty;

        /**
         * Calculates optimal shot direction and power
         * Considers obstacles, river force, and difficulty settings
         */
        public (Vector2 direction, float power) CalculateShot(Vector2 ballPosition, Vector2 holePosition, List<RotatingObstacle> obstacles, Level currentLevel)
        {
            _currentObstacles = obstacles;
            _currentLevel = currentLevel;

            List<Vector2> path = FindPath(ballPosition, holePosition, obstacles);

            if (difficulty == AIDifficulty.Hard)
            {
                path = OptimisePath(path, ballPosition, holePosition);
            }

            if (path == null || path.Count < 2)
            {
                float directDistance = Vector2.Distance(ballPosition, holePosition);
                float power = CalculateRequiredPower(directDistance);
                return (Vector2.Normalize(holePosition - ballPosition), power);
            }

            Vector2 targetPoint = path[1];
            if (difficulty == AIDifficulty.Hard && path.Count > 2)
            {
                float distanceToNext = Vector2.Distance(ballPosition, targetPoint);
                if (distanceToNext < 100 && CanDirectlyReach(ballPosition, path[2]))
                {
                    targetPoint = path[2];
                }
            }

            Vector2 direction = Vector2.Normalize(targetPoint - ballPosition);
            float distance = Vector2.Distance(ballPosition, targetPoint);
            float idealPower = CalculateRequiredPower(distance);

            if (difficulty == AIDifficulty.Hard)
            {
                direction = AddInaccuracy(direction, HARD_ACCURACY);
                idealPower *= HARD_POWER_CONTROL;
            }
            else
            {
                direction = AddInaccuracy(direction, EASY_ACCURACY);
                idealPower = AddPowerInaccuracy(idealPower, EASY_ACCURACY);
            }

            return (direction, idealPower);
        }

        /**
         * Optimises path by removing unnecessary nodes
         * Maintains obstacle avoidance whilst reducing path complexity
         */
        private List<Vector2> OptimisePath(List<Vector2> originalPath, Vector2 ballPosition, Vector2 holePosition)
        {
            if (originalPath == null || originalPath.Count <= 2)
                return originalPath;

            var optimisedPath = new List<Vector2> { originalPath[0] };
            int current = 0;

            while (current < originalPath.Count - 1)
            {
                int furthest = current + 1;
                for (int i = current + 2; i < originalPath.Count; i++)
                {
                    if (CanDirectlyReach(originalPath[current], originalPath[i]))
                    {
                        furthest = i;
                    }
                }
                optimisedPath.Add(originalPath[furthest]);
                current = furthest;
            }

            return optimisedPath;
        }

        /**
         * Calculates required power for shot distance
         * Adjusts for river force and difficulty settings
         */
        private float CalculateRequiredPower(float distance)
        {
            if (difficulty == AIDifficulty.Hard)
            {
                float basePower = distance * 0.15f;
                float riverFactor = 1.0f;

                if (_currentLevel?.RiverForce != Vector2.Zero)
                {
                    riverFactor = 1.2f;
                }

                return MathHelper.Clamp(basePower * riverFactor, MAX_POWER * HARD_MIN_POWER, MAX_POWER);
            }

            return MathHelper.Clamp(distance * 0.15f, 0, MAX_POWER);
        }

        private Vector2 AddInaccuracy(Vector2 direction, float accuracy)
        {
            float maxAngleError = MathHelper.Pi * (1 - accuracy);
            float angleError = (float)(random.NextDouble() - 0.5) * maxAngleError;

            return Vector2.Transform(direction, Matrix.CreateRotationZ(angleError));
        }

        private float AddPowerInaccuracy(float idealPower, float accuracy)
        {
            float maxPowerError = MAX_POWER * (1 - accuracy);
            float powerError = (float)(random.NextDouble() - 0.5) * maxPowerError;

            return MathHelper.Clamp(idealPower + powerError, 0, MAX_POWER);
        }

        private bool CanDirectlyReach(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float distance = direction.Length();
            direction.Normalize();

            // Check several points along the path
            for (float d = 0; d < distance; d += 20f)
            {
                Vector2 point = from + direction * d;
                foreach (var obstacle in _currentObstacles)
                {
                    if (Vector2.Distance(point, obstacle.Position) < obstacle.Size.Length() / 2 + 30)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private List<Vector2> FindPath(Vector2 start, Vector2 goal, List<RotatingObstacle> obstacles)
        {
            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();
            var startNode = new Node(start);

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(n => n.F).First();

                if (Vector2.Distance(current.Position, goal) < GOAL_THRESHOLD)
                {
                    return ReconstructPath(current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, goal, obstacles))
                {
                    if (closedSet.Any(n => Vector2.Distance(n.Position, neighbor.Position) < NODE_SPACING))
                        continue;

                    float tentativeG = current.G + Vector2.Distance(current.Position, neighbor.Position);

                    var existingNode = openSet.FirstOrDefault(n =>
                        Vector2.Distance(n.Position, neighbor.Position) < NODE_SPACING);

                    if (existingNode == null)
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeG >= existingNode.G)
                    {
                        continue;
                    }

                    neighbor.Parent = current;
                    neighbor.G = tentativeG;
                    neighbor.H = Vector2.Distance(neighbor.Position, goal);
                }
            }

            return null;
        }

        private List<Node> GetNeighbors(Node node, Vector2 goal, List<RotatingObstacle> obstacles)
        {
            var neighbors = new List<Node>();
            float[] angles = { 0, MathHelper.PiOver4, MathHelper.PiOver2, 3 * MathHelper.PiOver4,
                             MathHelper.Pi, -3 * MathHelper.PiOver4, -MathHelper.PiOver2, -MathHelper.PiOver4 };

            foreach (float angle in angles)
            {
                Vector2 direction = Vector2.Transform(Vector2.UnitX, Matrix.CreateRotationZ(angle));
                Vector2 newPos = node.Position + direction * NODE_SPACING;

                // Check if position is within bounds
                if (newPos.X < 50 || newPos.X > 1210 || newPos.Y < 100 || newPos.Y > 600)
                    continue;

                // Check obstacle collisions
                bool collision = false;
                foreach (var obstacle in obstacles)
                {
                    if (Vector2.Distance(newPos, obstacle.Position) < obstacle.Size.Length() / 2 + 20)
                    {
                        collision = true;
                        break;
                    }
                }

                if (!collision)
                {
                    var neighbor = new Node(newPos);
                    neighbors.Add(neighbor);
                }
            }

            // Add direct path to goal if close enough and reachable
            if (Vector2.Distance(node.Position, goal) < NODE_SPACING * 2 && CanDirectlyReach(node.Position, goal))
            {
                neighbors.Add(new Node(goal));
            }

            return neighbors;
        }

        private List<Vector2> ReconstructPath(Node endNode)
        {
            var path = new List<Vector2>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }

    public class Node
    {
        public Vector2 Position { get; set; }
        public float G { get; set; } // Cost from start
        public float H { get; set; } // Estimated cost to goal
        public float F => G + H;     // Total cost
        public Node Parent { get; set; }
        public Node(Vector2 position)
        {
            Position = position;
            G = 0;
            H = 0;
        }
    }
}