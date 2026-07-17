using Microsoft.Playwright;

namespace StudentRegistration.E2ETests.Specs.Spec015;

internal static class Spec015BrowserData
{
    public const string SubmissionId = "00000000-0000-0000-0000-000000015001";
    public const string ClientRequestId = "00000000-0000-0000-0000-000000015002";
    public const string TermId = "00000000-0000-0000-0000-000000015003";
    public const string MeetingId = "00000000-0000-0000-0000-000000015004";
    public const string ArchivedSubmissionId = "00000000-0000-0000-0000-000000015005";

    public static string AppContext => $$$"""
        {
          "serverTimeUtc":"2026-07-20T07:15:00Z","timeZoneId":"Africa/Cairo",
          "teachingTerm":null,
          "registrationTerm":{"id":"{{{TermId}}}","code":"FALL-2026","label":"Fall 2026","state":"registrationOpen","rowVersion":"TERM-RV-1"},
          "registrationWindowState":"open",
          "registrationWindow":{"id":"00000000-0000-0000-0000-000000015010","state":"open","opensAtUtc":"2026-07-19T06:00:00Z","closesAtUtc":"2026-07-21T18:00:00Z","rowVersion":"WINDOW-RV-1"},
          "serviceState":"available","displayName":"Synthetic Student One",
          "authorizedRoles":["Student"],"activeRole":"Student","sessionState":"active",
          "expiresAtUtc":"2026-07-20T09:15:00Z","supportReferencePath":"/support/student/STU-015-SAFE"
        }
        """;

    public static string AcceptedDetail => $$$"""
        {
          "status":"accepted",
          "receipt":{
            "submissionId":"{{{SubmissionId}}}","reference":"REG-2026-015001",
            "term":{"id":"{{{TermId}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},
            "submittedAtUtc":"2026-07-20T07:15:01Z","resultCode":"REGISTERED","policyVersion":"DEMO-POC-2026.1",
            "groups":[{
              "offeringId":"00000000-0000-0000-0000-000000015020","courseCode":"AI401","subjectTitle":"Artificial Intelligence",
              "groupId":"00000000-0000-0000-0000-000000015021","groupCode":"G01","credits":3,
              "meetings":[{
                "meetingId":"{{{MeetingId}}}","activityType":"Lecture","dayOfWeek":1,"startLocal":"10:00","endLocal":"11:30",
                "roomCode":"A-101","location":"Smart Village","staff":[
                  {"role":"Lecturer","displayName":"Dr. Salma"},{"role":"TeachingAssistant","displayName":"TA Noor"}
                ]
              }]
            }],
            "totalCredits":3
          },
          "rejection":null
        }
        """;

    public static string RejectedDetail => $$$"""
        {
          "status":"rejected","receipt":null,
          "rejection":{
            "submissionId":"{{{SubmissionId}}}","status":"rejected","resultCode":"GROUP_FULL",
            "safeMessage":"The selected group became full. Choose another group and review the plan again.",
            "submittedAtUtc":"2026-07-20T07:15:00Z","completedAtUtc":"2026-07-20T07:15:01Z","noPartialRegistration":true
          }
        }
        """;

    public static string History => $$$"""
        {
          "items":[
            {"submissionId":"{{{SubmissionId}}}","reference":"REG-2026-015001","term":{"id":"{{{TermId}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},"termState":"registrationOpen","status":"accepted","submittedAtUtc":"2026-07-20T07:15:01Z","groupCount":1,"totalCredits":3},
            {"submissionId":"{{{ArchivedSubmissionId}}}","reference":"REG-2025-015005","term":{"id":"00000000-0000-0000-0000-000000015099","code":"FALL-2025","displayName":"Fall 2025","timeZoneId":"Africa/Cairo"},"termState":"archived","status":"accepted","submittedAtUtc":"2025-07-20T07:15:01Z","groupCount":1,"totalCredits":3}
          ],
          "page":1,"pageSize":20,"totalCount":2,"sort":"submittedAtUtc desc,submissionId"
        }
        """;

    public static string EmptyHistory =>
        "{\"items\":[],\"page\":1,\"pageSize\":20,\"totalCount\":0,\"sort\":\"submittedAtUtc desc,submissionId\"}";

    public static string Timetable => $$$"""
        {
          "term":{"id":"{{{TermId}}}","code":"FALL-2026","displayName":"Fall 2026","timeZoneId":"Africa/Cairo"},
          "termState":"registrationOpen","registrationWindowState":"open",
          "groups":[{
            "offeringId":"00000000-0000-0000-0000-000000015020","courseCode":"AI401","subjectTitle":"Artificial Intelligence",
            "groupId":"00000000-0000-0000-0000-000000015021","groupCode":"G01","credits":3,
            "meetings":[{"meetingId":"{{{MeetingId}}}","activityType":"Lecture","dayOfWeek":1,"startLocal":"10:00","endLocal":"11:30","roomCode":"A-101","location":"Smart Village","staff":[{"role":"Lecturer","displayName":"Dr. Salma"},{"role":"TeachingAssistant","displayName":"TA Noor"}]}]
          }],
          "subjectDiscoveryPath":"/student/subjects"
        }
        """;

    public static string EmptyTimetable =>
        "{\"term\":{\"id\":\"" + TermId + "\",\"code\":\"FALL-2026\",\"displayName\":\"Fall 2026\",\"timeZoneId\":\"Africa/Cairo\"},\"termState\":\"registrationOpen\",\"registrationWindowState\":\"open\",\"groups\":[],\"subjectDiscoveryPath\":\"/student/subjects\"}";

    public static string Error(string code, string message = "The registration record is temporarily unavailable.") =>
        $"{{\"code\":\"{code}\",\"message\":\"{message}\",\"correlationId\":\"STU-015-SAFE\"}}";

    public static Task JsonAsync(IRoute route, int status, string body) =>
        route.FulfillAsync(new RouteFulfillOptions
        {
            Status = status,
            ContentType = "application/json",
            Body = body
        });

    public static string Path(IRoute route) => new Uri(route.Request.Url).AbsolutePath;
}
