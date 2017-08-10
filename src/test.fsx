  #r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
  #r "../packages/Suave/lib/net40/Suave.dll"
  #r "bin/debug/ApiAiSDK.dll"
  #r "bin/debug/Newtonsoft.Json.dll"
  #r "bin/debug/WebDriver.dll"
  #r "bin/debug/canopy.dll"

  #load "ParsingHelpers.fs"
  #load "CallbackAPI.fs"
  #load "SendAPI.fs"
  #load "API.fs"
  #load "MessageParsing.fs"



  open FacebookMessengerConnector
  open FacebookMessengerConnector.CallbackAPI
  open FacebookMessengerConnector.SendAPI
  open Suave


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

  open Suave.Logging
  let logger = Loggers.ConsoleWindowLogger LogLevel.Verbose

  startWebServer {defaultConfig with logger = logger} app


