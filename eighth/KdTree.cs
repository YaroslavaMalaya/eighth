namespace eighth;

public class KdNode
{
    public User User { get; set; }
    public List<double> Point { get; } 
    public KdNode Left { get; set; } 
    public KdNode Right { get; set; } 

    public KdNode(User user, List<double> point)
    {
        User = user;
        Point = point;
        Left = null;
        Right = null;
    }
}

public class KdTree
{
    private KdNode root; 
    public KdTree(List<User> users)
    {
        var points = users.Select(x => x.GenresRatings).ToList();
        root = BuildKdTree(points, 0, users); 
    }
    
    private KdNode BuildKdTree(List<List<double>>  points, int depth, List<User> usersss) 
    {
        if (points.Count == 0)
            return null;  

        var k = points[0].Count;  
        var axis = depth % k;  

        points.Sort((a, b) => a[axis].CompareTo(b[axis]));  

        var medianIndex = points.Count / 2;  
        var node = new KdNode(usersss[medianIndex], points[medianIndex]);  

        var leftPoints = points.GetRange(0, medianIndex);  
        var leftUsers = usersss.GetRange(0, medianIndex); 
        var rightPoints = points.GetRange(medianIndex + 1, points.Count - medianIndex - 1);  
        var rightUsers = usersss.GetRange(medianIndex + 1, points.Count - medianIndex - 1); 


        node.Left = BuildKdTree(leftPoints, depth + 1, leftUsers);  
        node.Right = BuildKdTree(rightPoints, depth + 1, rightUsers);  

        return node; 
    }

    public User FindNearestNeighbor(List<double> target)
    {
        KdNode nearestNode = FindNearestNeighbor(root, target, 0);  
        return nearestNode.User;  
    }

    private KdNode FindNearestNeighbor(KdNode node, List<double>  target, int depth)
    {
        if (node == null)
            return null;  

        var k = target.Count;  
        var axis = depth % k; 

        var currentBest = node; 
        KdNode nextBranch = null; 
        KdNode oppositeBranch = null; 

        if (target[axis] < node.Point[axis])
        {
            nextBranch = node.Left;  
            oppositeBranch = node.Right;  
        }
        else
        {
            nextBranch = node.Right;  
            oppositeBranch = node.Left; 
        }

        var closerNode = FindNearestNeighbor(nextBranch, target, depth + 1);  
        if (closerNode != null && Distance(target, closerNode.Point) < Distance(target, currentBest.Point))
            currentBest = closerNode;  

        if (ShouldCheckOppositeSubtree(target, currentBest.Point, axis))
        {
            var closerOppositeNode = FindNearestNeighbor(oppositeBranch, target, depth + 1);  
            if (closerOppositeNode != null && Distance(target, closerOppositeNode.Point) < Distance(target, currentBest.Point))
                currentBest = closerOppositeNode;  
        }

        return currentBest;  
    }
    
    private double Distance(List<double> a, List<double> b)
    {
        double sum = 0;
        for (var i = 0; i < a.Count; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }

    private bool ShouldCheckOppositeSubtree(List<double>  target, List<double>  currentBest, int axis)
    {
        var axisDist = target[axis] - currentBest[axis]; 
        return axisDist * axisDist < Distance(target, currentBest);
    }
}