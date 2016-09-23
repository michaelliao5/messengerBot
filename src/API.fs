namespace FacebookMessengerConnector

[<RequireQualifiedAccess>]
module Request =
   open FSharp.Data
   open FSharp.Data.HttpRequestHeaders
   open SendAPI

   let make access_token (r : Request) =
      Http.AsyncRequestString(
         "https://graph.facebook.com/v2.6/me/messages",
         silentHttpErrors = true,
         headers = [ ContentType HttpContentTypes.Json ],
         query = [ "access_token", access_token ],
         body = TextRequest (string <| Request.toJson r) )



[<RequireQualifiedAccess>]
module Callback =
   open Suave
   open Suave.Filters
   open Suave.Operators
   open Suave.Successful

   open FSharp.Data
   open CallbackAPI

   let webPart urlPath verifyToken handler =
      path urlPath >=> choose[
         // Page subscription verification is sent via GET requests
         GET >=>
            request (fun r ->
            match r.queryParam("hub.mode"), r.queryParam("hub.verify_token"), r.queryParam("hub.challenge") with
            | (Choice1Of2 "subscribe"), (Choice1Of2 vt), (Choice1Of2 challenge) when vt = verifyToken ->
               OK challenge
            | _ ->
               OK "Something went wrong"
            )

         // Callbacks are sent via POST requests
         POST >=>
            request (fun r ->
            let data, _ = r.form.[0]
            let json = JsonValue.Parse(data)
            let callback = Callback.fromJson json
            match callback with
            | ParsingHelpers.ParseResult.Success cb ->
               handler cb
            | _ ->
               ()
            ok [||])
      ]

