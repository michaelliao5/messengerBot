namespace FacebookMessengerConnector

open SendAPI
open ApiAiSDK
open ApiAiSDK.Model

module MessageParsing =
  open FSharp.Data
  open Newtonsoft.Json

  let aiToken = new AIConfiguration("6adee404ca74426e9476a9e56acd32f9", SupportedLanguage.English);
  let apiAi = new ApiAi(aiToken)


  let ProcessMessage (input:string) : Message = 
    let msg = apiAi.TextRequest(input).Result.Fulfillment.Speech
    printfn "msg: %s" msg
    Message.Text(msg, None, None)