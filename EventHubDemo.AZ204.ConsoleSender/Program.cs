using System;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

const string endpoint = "https://pkolosovcustomtopic.centralus-1.eventgrid.azure.net/api/events";
const string key = "bXJtk8I3t/Shd21ePMDP9UHpT0+SZb0xHtzVje5t8AE=";
var topicHostName = new Uri(endpoint).Host;

var acct = new EventGridEvent(
    id: Guid.NewGuid().ToString(),
    subject: "New Account",
    data: new { Message = "hi" },
    eventType: "NewAccountCreated",
    eventTime: DateTime.Now,
    dataVersion: "1.0");

var credentials = new TopicCredentials(key);

var client = new EventGridClient(credentials);

await client.PublishEventsAsync(topicHostName, new[] { acct });

Console.ReadKey();