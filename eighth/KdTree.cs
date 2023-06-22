namespace eighth;

using System;
    using System.Collections.Generic;
    public class KdTreeNode
    {
        public double[] Point { get; set; }
        public User User { get; set; }
        public KdTreeNode Left { get; set; }
        public KdTreeNode Right { get; set; }
    }

    public class KdTree
    {
        private KdTreeNode root;

        public void BuildTree(List<User> users)
        {
            if (users == null || users.Count == 0)
                return;

            var nodes = new List<KdTreeNode>();

            foreach (User user in users)
            {
                double[] point = GetUserPoint(user);
                KdTreeNode node = new KdTreeNode { Point = point, User = user };
                nodes.Add(node);
            }

            root = BuildKdTree(nodes, 0, nodes.Count - 1, 0);
        }

        private KdTreeNode BuildKdTree(List<KdTreeNode> nodes, int start, int end, int depth)
        {
            if (start > end)
                return null;

            var axis = depth % nodes[0].Point.Length;
            nodes.Sort((a, b) => a.Point[axis].CompareTo(b.Point[axis]));

            var medianIndex = start + (end - start) / 2;
            KdTreeNode medianNode = nodes[medianIndex];

            medianNode.Left = BuildKdTree(nodes, start, medianIndex - 1, depth + 1);
            medianNode.Right = BuildKdTree(nodes, medianIndex + 1, end, depth + 1);

            return medianNode;
        }

        public User FindNearestNeighbor(User user)
        {
            if (root == null || user == null)
                return null;

            double[] targetPoint = GetUserPoint(user);
            KdTreeNode nearestNode = FindNearestNode(root, targetPoint, 0);

            return nearestNode != null ? nearestNode.User : null;
        }

        private KdTreeNode FindNearestNode(KdTreeNode node, double[] targetPoint, int depth)
        {
            if (node == null)
                return null;

            var axis = depth % targetPoint.Length;

            KdTreeNode bestNode = node;
            var bestDistance = CalculateDistance(node.Point, targetPoint);

            KdTreeNode nearChild = node.Left;
            KdTreeNode farChild = node.Right;

            if (targetPoint[axis] > node.Point[axis])
            {
                nearChild = node.Right;
                farChild = node.Left;
            }

            KdTreeNode nearestNode = FindNearestNode(nearChild, targetPoint, depth + 1);

            if (nearestNode != null)
            {
                double nearestDistance = CalculateDistance(nearestNode.Point, targetPoint);
                if (nearestDistance < bestDistance)
                {
                    bestNode = nearestNode;
                    bestDistance = nearestDistance;
                }
            }

            if (IsWorthLookingFurther(node, targetPoint, bestDistance))
            {
                KdTreeNode furtherNode = FindNearestNode(farChild, targetPoint, depth + 1);
                if (furtherNode != null)
                {
                    var furtherDistance = CalculateDistance(furtherNode.Point, targetPoint);
                    if (furtherDistance < bestDistance)
                    {
                        bestNode = furtherNode;
                        bestDistance = furtherDistance;
                    }
                }
            }

            return bestNode;
        }

        private double CalculateDistance(double[] point1, double[] point2)
        {
            if (point1.Length != point2.Length)
                throw new ArgumentException("Points must have the same dimensionality.");

            double sumOfSquares = 0;
            for (var i = 0; i < point1.Length; i++)
            {
                var diff = point1[i] - point2[i];
                sumOfSquares += diff * diff;
            }

            return Math.Sqrt(sumOfSquares);
        }

        private double[] GetUserPoint(User user)
        {
            var point = new double[user.MoviesByGenres.Count];

            var i = 0;
            foreach (var genre in user.MoviesByGenres)
            {
                // способ для получения значения предпочтения
                // пользователя для жанра
                double preference = genre.Value.Count;
                point[i] = preference;
                i++;
            }

            return point;
        }

        private bool IsWorthLookingFurther(KdTreeNode node, double[] targetPoint, double bestDistance)
        {
            if (node == null)
                return false;

            var axisDistance = targetPoint[node.Point.Length % targetPoint.Length] - node.Point[node.Point.Length % targetPoint.Length];
            return Math.Abs(axisDistance) < bestDistance;
        }
}

    