using Azure.AI.OpenAI;
using ChatCompletion.Model;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;
using System.Net.Quic;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;


#region Azure Open AI - Assistant to Extract data from PDF
var builder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


IConfiguration config = builder.Build();

//Azure OpenAI service credentials
string openAiEndpoint = config["AzureOpenAI:Endpoint"]!;
string openAiApiKey = config["AzureOpenAI:ApiKey"]!;
string deploymentName = config["AzureOpenAI:DeploymentName"]!;

//Sample pdf file url
string filePath = @"https://storage.googleapis.com/akute-ehr-documents-dev/66d1559db301ef0008a8d6ee%2Fd25edc1c-9fa6-4611-914e-c7d3e56f8723%2FDiagnostic_Report-sathiya%20s-2025_05_02_1%3A25%3A13%3A042.pdf?GoogleAccessId=ehr-user%40akute-ehr-dev-253816.iam.gserviceaccount.com&Expires=1747467122&Signature=W4uzno1kifqe9%2Fk6rn3Xo17HaEYdBXieCaoKNtvFmVLlOFmbk0mj6wwYKgH%2BYSLzMi3xJeqCJJ8ihhPbSolUj9rmWqGLl64z1UTFVAjSX2qlBdkRMNRBo01SfgzYV4kLzQKF73Vk4kKGm7y2IFLl5itbnVhS2hv%2FB%2FIR%2FeXltexYvSgk2O%2Fbvy6vptHNBCVQZYvPFYx9uFg7poAHMRGrvNY9J8jrB6sLx5CpC3q6mqn%2BE25Y%2F4MYM8VfzMjwSRGVoohHtUUNgMYXTK7n%2B8FsqyTcq7%2B9VXKdR2vr4WCo%2BXXwjTVCvW9UxDnnY%2FtqW8%2BWnYCQxB0doj%2FFOBIltBH4XA%3D%3D";

//Read the file contant from the url
string pdfText = ExtractTextFromPdf(filePath);

//Here you can ask the question based on the pdf content at run time also. For example, you can ask "What is the patient name?" or "What is the diagnosis?" etc.

//string? question = Console.ReadLine();
//question = question ?? "Return the patient name, dob, gender and phone in json format";

//Default question. 
string question = "Return the patient name, dob, gender and phone in json format";
Console.WriteLine($"your question:{question}");

//string question = Console.ReadLine();

//Create the prompt with pdf content and user question
string prompt = $"You are a helpful assistant. Use the PDF content below to answer the question.\n\nPDF Content:\n{pdfText}\n\nQuestion: {question}";

//Send question + context to Azure OpenAI
string response = GetResponseFromAzureOpenAI(openAiEndpoint, openAiApiKey, deploymentName, prompt);

//Actual Response from OpenIAi
Console.WriteLine("\nAssistant Response:");
Console.WriteLine(response);

var responseObj = JsonSerializer.Deserialize<ResponseObj>(response);

Console.WriteLine("------------------------------------------------");

Console.WriteLine($"Patient Name: {responseObj?.patient_name}");
Console.WriteLine("Phone:" + responseObj?.phone);
Console.WriteLine("Gender:" + responseObj?.gender);
Console.WriteLine("DOB:" + responseObj?.dob);
#endregion

#region Extract contant from PDF
static string ExtractTextFromPdf(string filePath)
{
    var sb = new StringBuilder();

    using (var reader = new PdfReader(filePath))
    using (var pdfDoc = new PdfDocument(reader))
    {
        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
        {
            var strategy = new SimpleTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
            sb.AppendLine(text);
        }
    }

    return sb.ToString();
}
#endregion

#region Chat with Open AI
static string GetResponseFromAzureOpenAI(string endpoint, string apiKey, string deploymentName, string prompt)
{
    //Creating the Azure Open AI Client using endpoint and key
    AzureOpenAIClient azureClient = new(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey));

    //Create the chant instance
    ChatClient chatClient = azureClient.GetChatClient(deploymentName);

    //Request Options
    var requestOptions = new ChatCompletionOptions()
    {
        MaxOutputTokenCount = 1000,
        Temperature = 1.0f,
        TopP = 1.0f,
        FrequencyPenalty = 0.0f,
        PresencePenalty = 0.0f,
    };
    //Add the prompt into ChatMessage
    List<ChatMessage> messages = new List<ChatMessage>()
            {   new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage(prompt),
            };

    //Execute the Chat
    var response = chatClient.CompleteChat(messages, requestOptions);

    //Send the response string
    return response.Value.Content[0].Text;

}
#endregion