namespace Shop_API.Logics.Helpers
{
    public class CategoryTree
    {
        private readonly Dictionary<int, int?> _parents = new();
        private readonly Dictionary<int, List<int>> _children = new();

        public CategoryTree(IEnumerable<(int Id, int? ParentId)> nodes)
        {
            var list = nodes.ToList();

            foreach (var node in list)
            {
                _parents[node.Id] = node.ParentId;
            }

            foreach (var node in list)
            {
                if (!node.ParentId.HasValue || !_parents.ContainsKey(node.ParentId.Value))
                {
                    continue;
                }

                if (!_children.TryGetValue(node.ParentId.Value, out var children))
                {
                    children = new List<int>();

                    _children[node.ParentId.Value] = children;
                }

                children.Add(node.Id);
            }
        }

        public bool Contains(int id)
        {
            return _parents.ContainsKey(id);
        }

        public List<int> GetAncestorIds(int id)
        {
            var result = new List<int>();

            var visited = new HashSet<int> { id };

            var current = _parents.GetValueOrDefault(id);

            while (current.HasValue && visited.Add(current.Value))
            {
                result.Add(current.Value);

                current = _parents.GetValueOrDefault(current.Value);
            }

            result.Reverse();

            return result;
        }

        public List<int> GetDescendantIds(int id)
        {
            var result = new List<int>();

            var visited = new HashSet<int> { id };

            var stack = new Stack<int>();

            stack.Push(id);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (!_children.TryGetValue(current, out var children))
                {
                    continue;
                }

                foreach (var child in children)
                {
                    if (visited.Add(child))
                    {
                        result.Add(child);

                        stack.Push(child);
                    }
                }
            }

            return result;
        }

        public int GetDepth(int id)
        {
            return GetAncestorIds(id).Count + 1;
        }

        public int GetHeight(int id)
        {
            var height = 0;

            var level = new List<int> { id };

            var visited = new HashSet<int> { id };

            while (level.Count > 0)
            {
                height++;

                var next = new List<int>();

                foreach (var current in level)
                {
                    if (!_children.TryGetValue(current, out var children))
                    {
                        continue;
                    }

                    foreach (var child in children)
                    {
                        if (visited.Add(child))
                        {
                            next.Add(child);
                        }
                    }
                }

                level = next;
            }

            return height;
        }
    }
}
