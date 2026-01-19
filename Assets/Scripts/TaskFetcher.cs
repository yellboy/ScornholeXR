using Meta.XR.BuildingBlocks.AIBlocks;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class TaskFetcher : MonoBehaviour
{

    [SerializeField] private LlmAgent _llmAgent;

    [SerializeField] private TMP_Text _queryText;

    private void Start()
    {
        FetchTask();
    }

    private void FetchTask()
    {
        var prompt = @"
I want to simulate a scornhole game from Good Mythical Morning. You need to send me a randomized task in JSON format. 
The task should consist of a question and 5 options to choose from. The question should be related to pop culture, food, or general knowledge and based on some real source.
The question should be something that can be listed. There is no single correct answer, but rather a ranking of the options based on popularity or preference.
The question should be asked in a negative manner. Meaning, what is the least popular, least loved, least favorite, least ordered, etc.
There should also be an answer, listing the options in the correct order, from least favorite to most favorite.
Example of a task:

{
 ""Question"": ""What is the least popular condiment?"",
 ""Option1"": ""Ketchup"",
 ""Option2"": ""Mustard"",
 ""Option3"": ""Mayo"",
 ""Option4"": ""Barbeque sauce"",
 ""Option5"": ""Ranch"",
 ""Rank"": [4, 2, 3, 5, 1],
 ""Source"": ""Some source"",
}

In the example above, barbeque sauce is the least popular condiment, followed by mustard, mayo, ketchup, and ranch as the most popular.
Answer only with the task in the format I described and nothing else.
Always provide only a task and nothing else.";

        _llmAgent.SendPromptAsync(prompt);
    }

    public void OnResponseReceived()
    {
        var lastResponse = _llmAgent.History.LastOrDefault();
        if (lastResponse == null)
        {
            return;
        }

        var jsonTask = lastResponse.Replace("Assistant:", "");
        var task = JsonConvert.DeserializeObject<TaskObject>(jsonTask);

        _queryText.text = 
@$"{task.Question}
1. {task.Option1}
2. {task.Option2}
3. {task.Option3}
4. {task.Option4}
5. {task.Option5}";
    }
}
