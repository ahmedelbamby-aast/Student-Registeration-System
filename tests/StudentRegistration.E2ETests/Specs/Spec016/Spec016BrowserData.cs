using Microsoft.Playwright;

namespace StudentRegistration.E2ETests.Specs.Spec016;

internal static class Spec016BrowserData
{
    internal const string GroupId = "00000000-0000-0000-0000-000000016001";
    internal const string MeetingId = "00000000-0000-0000-0000-000000016002";
    internal const string AvailabilityId = "00000000-0000-0000-0000-000000016010";
    internal const string RangeId = "00000000-0000-0000-0000-000000016011";

    internal static string Context(string activeRole = "Lecturer") => $$$"""
    {"serverTimeUtc":"2026-07-20T07:15:00Z","timeZoneId":"Africa/Cairo","teachingTerm":null,
    "registrationTerm":null,"registrationWindowState":"none","registrationWindow":null,
    "serviceState":"available","displayName":"Dr. Salma","authorizedRoles":["{{{activeRole}}}"],
    "activeRole":"{{{activeRole}}}","sessionState":"active","expiresAtUtc":"2026-07-20T09:15:00Z","supportReferencePath":"/support/staff"}
    """;

    internal static string Assignment => $$$"""
    {"subjectCode":"AI401","subjectTitle":"Artificial Intelligence","group":{
    "id":"{{{GroupId}}}","offeringId":"00000000-0000-0000-0000-000000016020","groupCode":"L01","capacity":30,
    "enrolledCount":2,"registrationPaused":false,"state":"published","selectable":true,"nonSelectableReasons":[],
    "staff":[{"meetingSlotId":"{{{MeetingId}}}","activityType":"lecture","staffId":"00000000-0000-0000-0000-000000016030","role":"Lecturer","name":"Dr. Salma"}],
    "meetings":[{"id":"{{{MeetingId}}}","activityType":"lecture","dayOfWeek":1,"startLocal":"09:00:00","endLocal":"10:30:00",
    "roomId":"00000000-0000-0000-0000-000000016040","roomCode":"A-101","location":"Smart Village"}],"rowVersion":"GROUP-RV-1"},
    "staffRole":"Lecturer","rosterCount":2}
    """;

    internal static string Assignments => $"[{Assignment}]";
    internal static string Timetable => $$$"""{"roleContext":"Lecturer","assignments":[{{{Assignment}}}]}""";
    internal static string Roster => """
    {"items":[{"universityId":"S016001","displayName":"Amina Hassan","enrollmentState":"active"},
    {"universityId":"S016002","displayName":"Youssef Ali","enrollmentState":"active"}],
    "page":1,"pageSize":20,"totalCount":2,"sort":"displayName:asc,universityId:asc"}
    """;
    internal static string Availability => $$$"""
    {"id":"{{{AvailabilityId}}}","staffId":"00000000-0000-0000-0000-000000016030",
    "termId":"00000000-0000-0000-0000-000000016050","deadlineUtc":"2026-07-21T18:00:00Z","rowVersion":"AV-RV-1",
    "ranges":[{"id":"{{{RangeId}}}","dayOfWeek":1,"startLocal":"09:00:00","endLocal":"12:00:00","kind":"available"}]}
    """;
    internal static string AvailabilityUpdated => $$$"""
    {"availability":{{{Availability}}},"impactAlertIds":["00000000-0000-0000-0000-000000016060"]}
    """;

    internal static string Error(string code, string message = "The request could not be completed safely.") =>
        $$$"""{"code":"{{{code}}}","message":"{{{message}}}","correlationId":"STF-016-SAFE"}""";

    internal static string AvailabilityConflict(string code) => $$$"""
    {"error":{"code":"{{{code}}}","message":"Availability changed.","correlationId":"STF-016-SAFE","currentVersion":"AV-RV-2"},
    "currentAvailability":{{{Availability}}},"serverTimeUtc":"2026-07-20T07:15:00Z","deadlineUtc":"2026-07-21T18:00:00Z"}
    """;

    internal static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;

    internal static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new() { Status = status, ContentType = "application/json", Body = body });
}
