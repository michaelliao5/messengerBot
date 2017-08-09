module FacebookMessengerConnector.Program

open System
open System.Net
open System.Threading
open FacebookMessengerConnector
open FacebookMessengerConnector.CallbackAPI
open FacebookMessengerConnector.SendAPI
open Suave
open Suave.Logging

[<EntryPoint>]
let main argv =   
  let send r =
     r |> Request.make "EAAU0BhKdRoQBAGZBGcsxJ4yJllbyIVnm2lnbjv2RU3P7rhzmLGnLTfxT1OP0qETAkgLN5UqRBZCDoerBX0o5WAttG4PmZATwQkEfhSdZCqC7vjC3ZB9YvsqqqqQxu7pKtWHF70NsXZAEXRMHL55zk7FQGICwsLZARCcUEOZAwONHPQZDZD"
     |> Async.RunSynchronously
     |> printfn "%A"

  let handler (callback : Callback) =
     for entry in callback.Entries do
        for event in entry.messagingEvents do
           match event.message with
           | None -> ()
           | Some m ->
              let action = SenderAction ((Recipient.Id event.sender), TypingOn)
              send action
              match m.text with
              | Some text ->
                 let response = Message((Recipient.Id event.sender), MessageParsing.ProcessMessage(text), None)
                 send response
              | _ -> ()



  let app = Callback.webPart "/webhook" "DONGDONG" handler

  
  let logger = Loggers.ConsoleWindowLogger LogLevel.Verbose

  startWebServer {defaultConfig with logger = logger} app


  let rec rep = async {
    //To be switched to nlog after
    Console.WriteLine("running")
    do! Async.Sleep(10000)
    return! rep
  }
  rep
  |> Async.RunSynchronously
  0