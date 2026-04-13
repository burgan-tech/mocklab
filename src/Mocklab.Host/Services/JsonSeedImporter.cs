using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mocklab.Host.Constants;
using Mocklab.Host.Data;
using Mocklab.Host.Models;

namespace Mocklab.Host.Services;

public class JsonSeedImporter(
    MocklabDbContext dbContext,
    ILogger<JsonSeedImporter> logger) : IJsonSeedImporter
{
    private readonly MocklabDbContext _dbContext = dbContext;
    private readonly ILogger<JsonSeedImporter> _logger = logger;

    private const string KeyValueOwnerTypeMockResponseRule = "MockResponseRule";

    public async Task<SeedImportResult> ImportAsync(JsonElement root, string sourceFile)
    {
        if (!root.TryGetProperty("collection", out var collectionData))
            return new SeedImportResult(true, "Missing 'collection' property", 0);

        if (!root.TryGetProperty("mocks", out var mocksData))
            return new SeedImportResult(true, "Missing 'mocks' property", 0);

        var collectionName = collectionData.TryGetProperty("name", out var nameEl)
            ? nameEl.GetString() ?? "Imported Collection"
            : "Imported Collection";

        var alreadyExists = await _dbContext.MockCollections
            .AnyAsync(c => c.Name == collectionName);

        if (alreadyExists)
        {
            _logger.LogDebug("Seed: skipping '{File}' — collection '{Name}' already exists.", sourceFile, collectionName);
            return new SeedImportResult(true, $"Collection '{collectionName}' already exists", 0);
        }

        var collection = new MockCollection
        {
            Name = collectionName,
            Description = collectionData.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            Color = collectionData.TryGetProperty("color", out var color) ? color.GetString() : null,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MockCollections.Add(collection);
        await _dbContext.SaveChangesAsync();

        var newFolderIds = new List<int>();
        if (root.TryGetProperty("folders", out var foldersData))
        {
            foreach (var folderEl in foldersData.EnumerateArray())
            {
                var name = folderEl.TryGetProperty("name", out var fn) ? fn.GetString() ?? "Folder" : "Folder";
                var folderColor = folderEl.TryGetProperty("color", out var fc) ? fc.GetString() : null;
                int? parentFolderId = null;
                if (folderEl.TryGetProperty("parentFolderIndex", out var p) && p.ValueKind == JsonValueKind.Number)
                {
                    var idx = p.GetInt32();
                    if (idx >= 0 && idx < newFolderIds.Count)
                        parentFolderId = newFolderIds[idx];
                }

                var folder = new MockFolder
                {
                    CollectionId = collection.Id,
                    ParentFolderId = parentFolderId,
                    Name = name,
                    Color = folderColor,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.MockFolders.Add(folder);
                await _dbContext.SaveChangesAsync();
                newFolderIds.Add(folder.Id);
            }
        }

        var seenMethodRoute = new HashSet<(string Method, string Route)>();
        var mocksToAdd = new List<MockResponse>();
        var rulesPerMock = new List<List<(MockResponseRule Rule, List<ResponseHeaderItem> Headers)>>();
        var sequencePerMock = new List<List<MockResponseSequenceItem>>();

        foreach (var mockData in mocksData.EnumerateArray())
        {
            var httpMethod = (mockData.TryGetProperty("httpMethod", out var hm) ? hm.GetString() ?? HttpConstants.MethodGet : HttpConstants.MethodGet)
                .Trim().ToUpperInvariant();
            var route = (mockData.TryGetProperty("route", out var rt) ? rt.GetString() ?? "/" : "/").Trim();

            if (!seenMethodRoute.Add((httpMethod, route)))
                continue;

            int? folderId = null;
            if (mockData.TryGetProperty("folderIndex", out var folderIndexEl) && folderIndexEl.ValueKind == JsonValueKind.Number)
            {
                var fi = folderIndexEl.GetInt32();
                if (fi >= 0 && fi < newFolderIds.Count)
                    folderId = newFolderIds[fi];
            }

            var mock = new MockResponse
            {
                HttpMethod = httpMethod,
                Route = route,
                QueryString = mockData.TryGetProperty("queryString", out var qs) ? qs.GetString() : null,
                RequestBody = mockData.TryGetProperty("requestBody", out var rb) ? rb.GetString() : null,
                StatusCode = mockData.TryGetProperty("statusCode", out var sc) && sc.ValueKind == JsonValueKind.Number ? sc.GetInt32() : 200,
                ResponseBody = mockData.TryGetProperty("responseBody", out var respBody) ? respBody.GetString() ?? "{}" : "{}",
                ContentType = mockData.TryGetProperty("contentType", out var ct) ? ct.GetString() ?? "application/json" : "application/json",
                Description = mockData.TryGetProperty("description", out var d) ? d.GetString() : null,
                DelayMs = mockData.TryGetProperty("delayMs", out var delay) && delay.ValueKind == JsonValueKind.Number ? delay.GetInt32() : null,
                IsActive = !mockData.TryGetProperty("isActive", out var ia) || ia.ValueKind != JsonValueKind.False,
                IsSequential = mockData.TryGetProperty("isSequential", out var seqFlag) && seqFlag.ValueKind == JsonValueKind.True,
                CollectionId = collection.Id,
                FolderId = folderId,
                CreatedAt = DateTime.UtcNow
            };
            mocksToAdd.Add(mock);

            var ruleList = new List<(MockResponseRule Rule, List<ResponseHeaderItem> Headers)>();
            if (mockData.TryGetProperty("rules", out var rulesEl) && rulesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var ruleEl in rulesEl.EnumerateArray())
                {
                    var headers = new List<ResponseHeaderItem>();
                    if (ruleEl.TryGetProperty("responseHeaders", out var headersEl) && headersEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var he in headersEl.EnumerateArray())
                        {
                            var key = he.TryGetProperty("key", out var k) ? k.GetString()?.Trim() : null;
                            if (string.IsNullOrEmpty(key)) continue;
                            headers.Add(new ResponseHeaderItem
                            {
                                Key = key!,
                                Value = he.TryGetProperty("value", out var v) ? v.GetString() ?? "" : ""
                            });
                        }
                    }

                    ruleList.Add((new MockResponseRule
                    {
                        ConditionField = ruleEl.TryGetProperty("conditionField", out var cf) ? cf.GetString() ?? "" : "",
                        ConditionOperator = ruleEl.TryGetProperty("conditionOperator", out var co) ? co.GetString() ?? "equals" : "equals",
                        ConditionValue = ruleEl.TryGetProperty("conditionValue", out var cv) ? cv.GetString() : null,
                        StatusCode = ruleEl.TryGetProperty("statusCode", out var rsc) && rsc.ValueKind == JsonValueKind.Number ? rsc.GetInt32() : 200,
                        ResponseBody = ruleEl.TryGetProperty("responseBody", out var ruleRb) ? ruleRb.GetString() ?? "{}" : "{}",
                        ContentType = ruleEl.TryGetProperty("contentType", out var ruleCt) ? ruleCt.GetString() ?? "application/json" : "application/json",
                        Priority = ruleEl.TryGetProperty("priority", out var pr) && pr.ValueKind == JsonValueKind.Number ? pr.GetInt32() : 0
                    }, headers));
                }
            }
            rulesPerMock.Add(ruleList);

            var seqList = new List<MockResponseSequenceItem>();
            if (mockData.TryGetProperty("sequenceItems", out var seqEl) && seqEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var siEl in seqEl.EnumerateArray())
                {
                    seqList.Add(new MockResponseSequenceItem
                    {
                        Order = siEl.TryGetProperty("order", out var o) && o.ValueKind == JsonValueKind.Number ? o.GetInt32() : 0,
                        StatusCode = siEl.TryGetProperty("statusCode", out var siSc) && siSc.ValueKind == JsonValueKind.Number ? siSc.GetInt32() : 200,
                        ResponseBody = siEl.TryGetProperty("responseBody", out var sb) ? sb.GetString() ?? "{}" : "{}",
                        ContentType = siEl.TryGetProperty("contentType", out var siCt) ? siCt.GetString() ?? "application/json" : "application/json",
                        DelayMs = siEl.TryGetProperty("delayMs", out var dm) && dm.ValueKind == JsonValueKind.Number ? dm.GetInt32() : null
                    });
                }
            }
            sequencePerMock.Add(seqList);
        }

        _dbContext.MockResponses.AddRange(mocksToAdd);
        await _dbContext.SaveChangesAsync();

        var allRules = new List<MockResponseRule>();
        var allRuleHeaders = new List<List<ResponseHeaderItem>>();
        for (var i = 0; i < mocksToAdd.Count; i++)
        {
            foreach (var (rule, headers) in rulesPerMock[i])
            {
                rule.MockResponseId = mocksToAdd[i].Id;
                allRules.Add(rule);
                allRuleHeaders.Add(headers);
            }
        }

        if (allRules.Count > 0)
        {
            _dbContext.MockResponseRules.AddRange(allRules);
            await _dbContext.SaveChangesAsync();

            for (var j = 0; j < allRules.Count; j++)
            {
                foreach (var h in allRuleHeaders[j])
                {
                    _dbContext.KeyValueEntries.Add(new KeyValueEntry
                    {
                        OwnerType = KeyValueOwnerTypeMockResponseRule,
                        OwnerId = allRules[j].Id,
                        Key = h.Key,
                        Value = h.Value
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        var allSeqItems = new List<MockResponseSequenceItem>();
        for (var i = 0; i < mocksToAdd.Count; i++)
        {
            foreach (var si in sequencePerMock[i])
            {
                si.MockResponseId = mocksToAdd[i].Id;
                allSeqItems.Add(si);
            }
        }

        if (allSeqItems.Count > 0)
        {
            _dbContext.MockResponseSequenceItems.AddRange(allSeqItems);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Seed: imported collection '{Name}' from '{File}' — {Count} mock(s).",
            collectionName, sourceFile, mocksToAdd.Count);

        return new SeedImportResult(false, null, mocksToAdd.Count);
    }
}
