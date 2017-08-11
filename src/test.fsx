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
     r |> Request.make "EAAU0BhKdRoQBAB2QzVG2TYEyw9ZAIYwJr83SdSr2zAwHqZBw1sN3E29ywMUd4njteNwpZA9ArsQTM2a8q8YR9W71hdNMT1gQihUuOT7PxS4qrKR05uiti7cT9fxPRLGKMZAsrQl7IsTq1Dk0AJRORAp3vrWyNea1FAcQZAsjKAAZDZD"
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
  let port = 8086
  let local = Suave.Http.HttpBinding.mkSimple HTTP "127.0.0.1" port
  startWebServer {defaultConfig with bindings = [local]; logger = logger} app


