namespace eighth;

public class KdNode
{
    public User User { get; set; }
    public List<double> Point { get; }  // Точка, яку вузол представляє
    public KdNode Left { get; set; }  // Посилання на лівого нащадка
    public KdNode Right { get; set; }  // Посилання на правого нащадка

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
    private KdNode root;  // Корінь дерева
    public KdTree(List<User> users)
    {
        var points = users.Select(x => x.GenresRatings).ToList();
        root = BuildKdTree(points, 0, users);  // Побудувати дерево приймаючи список точок
    }
    
    private KdNode BuildKdTree(List<List<double>>  points, int depth, List<User> usersss) 
    {
        if (points.Count == 0)
            return null;  // Якщо список точок порожній, повернути null

        var k = points[0].Count;  // Кількість вимірів точок
        var axis = depth % k;  // Визначити вимір, за яким проводиться поділ

        points.Sort((a, b) => a[axis].CompareTo(b[axis]));  // Відсортувати точки по вказаному виміру

        var medianIndex = points.Count / 2;  // Індекс медіани точок
        var node = new KdNode(usersss[medianIndex], points[medianIndex]);  // Створити вузол з точкою-медіаною

        var leftPoints = points.GetRange(0, medianIndex);  // Список точок лівої підгілля
        var leftUsers = usersss.GetRange(0, medianIndex); // Список юзерів лівої підгілля
        var rightPoints = points.GetRange(medianIndex + 1, points.Count - medianIndex - 1);  // Список точок правої підгілля
        var rightUsers = usersss.GetRange(medianIndex + 1, points.Count - medianIndex - 1); // Список юзерів правої підгілля


        node.Left = BuildKdTree(leftPoints, depth + 1, leftUsers);  // Рекурсивно побудувати ліве піддерево
        node.Right = BuildKdTree(rightPoints, depth + 1, rightUsers);  // Рекурсивно побудувати праве піддерево

        return node;  // Повернути побудований вузол
    }

    public User FindNearestNeighbor(List<double> target)
    {
        KdNode nearestNode = FindNearestNeighbor(root, target, 0);  // Знайти найближчий вузол до заданої точки
        return nearestNode.User;  // Повернути юзера найближчого вузла
    }

    private KdNode FindNearestNeighbor(KdNode node, List<double>  target, int depth)
    {
        if (node == null)
            return null;  // Якщо вузол є null, повернути null

        var k = target.Count;  // Кількість вимірів точки
        var axis = depth % k;  // Визначити вимір, за яким проводиться порівняння

        var currentBest = node;  // Поточно найближчий вузол
        KdNode nextBranch = null;  // Вузол наступної гілки (зліва або справа)
        KdNode oppositeBranch = null;  // Вузол протилежної гілки (справа або зліва)

        if (target[axis] < node.Point[axis])
        {
            nextBranch = node.Left;  // Якщо вимір цільової точки менший, обрати ліву гілку
            oppositeBranch = node.Right;  // Протилежна гілка буде правою
        }
        else
        {
            nextBranch = node.Right;  // Якщо вимір цільової точки більший або рівний, обрати праву гілку
            oppositeBranch = node.Left;  // Протилежна гілка буде лівою
        }

        var closerNode = FindNearestNeighbor(nextBranch, target, depth + 1);  // Рекурсивно знайти найближчий вузол в наступній гілці
        if (closerNode != null && Distance(target, closerNode.Point) < Distance(target, currentBest.Point))
            currentBest = closerNode;  // Якщо знайдений вузол ближчий до цільової точки, оновити поточно найближчий вузол

        if (ShouldCheckOppositeSubtree(target, currentBest.Point, axis))
        {
            var closerOppositeNode = FindNearestNeighbor(oppositeBranch, target, depth + 1);  // Рекурсивно знайти найближчий вузол в протилежній гілці
            if (closerOppositeNode != null && Distance(target, closerOppositeNode.Point) < Distance(target, currentBest.Point))
                currentBest = closerOppositeNode;  // Якщо знайдений вузол ближчий до цільової точки, оновити поточно найближчий вузол
        }

        return currentBest;  // Повернути поточно найближчий вузол
    }
    
    private double Distance(List<double> a, List<double> b)
    {
        double sum = 0;
        for (var i = 0; i < a.Count; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);  // Обчислити евклідову відстань між двома точками
    }

    private bool ShouldCheckOppositeSubtree(List<double>  target, List<double>  currentBest, int axis)
    {
        var axisDist = target[axis] - currentBest[axis];  // Відстань по виміру між цільовою точкою і поточно найближчим вузлом
        return axisDist * axisDist < Distance(target, currentBest);  // Перевірити, чи варто перевіряти протилежну гілку
    }
}