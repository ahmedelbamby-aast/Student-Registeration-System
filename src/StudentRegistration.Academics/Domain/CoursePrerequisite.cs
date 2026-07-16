namespace StudentRegistration.Academics.Domain;

public sealed class CoursePrerequisite
{
    private CoursePrerequisite()
    {
    }

    public CoursePrerequisite(
        Guid catalogueVersionId,
        Guid courseId,
        Guid requiredCourseId,
        string? minimumGrade,
        CatalogueFieldProvenance provenance)
    {
        DomainValue.Identifier(catalogueVersionId, nameof(catalogueVersionId));
        DomainValue.Identifier(courseId, nameof(courseId));
        DomainValue.Identifier(requiredCourseId, nameof(requiredCourseId));
        if (courseId == requiredCourseId)
        {
            throw new ArgumentException(
                "A course cannot require itself.",
                nameof(requiredCourseId));
        }

        CatalogueVersionId = catalogueVersionId;
        CourseId = courseId;
        RequiredCourseId = requiredCourseId;
        MinimumGrade = string.IsNullOrWhiteSpace(minimumGrade)
            ? null
            : minimumGrade.Trim().ToUpperInvariant();
        Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));
    }

    public Guid CatalogueVersionId { get; private set; }

    public Guid CourseId { get; private set; }

    public Guid RequiredCourseId { get; private set; }

    public string? MinimumGrade { get; private set; }

    public CatalogueFieldProvenance Provenance { get; private set; } = null!;

    public static void ValidateGraph(
        IEnumerable<Guid> courseIds,
        IEnumerable<CoursePrerequisite> prerequisites)
    {
        var courses = (courseIds ?? throw new ArgumentNullException(nameof(courseIds)))
            .ToHashSet();
        var edges = (prerequisites ?? throw new ArgumentNullException(nameof(prerequisites)))
            .ToArray();

        foreach (var edge in edges)
        {
            if (!courses.Contains(edge.CourseId) || !courses.Contains(edge.RequiredCourseId))
            {
                throw new InvalidOperationException(
                    $"Prerequisite reference is missing for {edge.CourseId:D} -> {edge.RequiredCourseId:D}.");
            }
        }

        var adjacency = edges
            .GroupBy(edge => edge.CourseId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(edge => edge.RequiredCourseId).ToArray());
        var visiting = new HashSet<Guid>();
        var visited = new HashSet<Guid>();
        var path = new List<Guid>();

        foreach (var courseId in courses.OrderBy(id => id))
        {
            Visit(courseId);
        }

        void Visit(Guid courseId)
        {
            if (visited.Contains(courseId))
            {
                return;
            }

            if (!visiting.Add(courseId))
            {
                var cycleStart = path.IndexOf(courseId);
                var cycle = path.Skip(Math.Max(cycleStart, 0)).Append(courseId);
                throw new InvalidOperationException(
                    $"Prerequisite cycle detected: {string.Join(" -> ", cycle.Select(id => id.ToString("D")))}.");
            }

            path.Add(courseId);
            if (adjacency.TryGetValue(courseId, out var required))
            {
                foreach (var requiredId in required.OrderBy(id => id))
                {
                    Visit(requiredId);
                }
            }

            path.RemoveAt(path.Count - 1);
            visiting.Remove(courseId);
            visited.Add(courseId);
        }
    }
}
