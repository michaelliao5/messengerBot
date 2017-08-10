namespace FacebookMessengerConnector

open SendAPI
open ApiAiSDK
open ApiAiSDK.Model
open System
open canopy
open System.Text.RegularExpressions
open runner

module MessageParsing =
  open FSharp.Data
  open Newtonsoft.Json
  open System.Text.RegularExpressions

  ///

  type Result<'a, 'b> = Choice<'a, 'b>

  /// Active pattern for matching a Choice<'a, 'b> with Choice1Of2 = Ok and Choice2Of2 = Error
  let inline (|Ok|Error|) c = c

  /// Indicates ok as Choice1Of2
  let inline Ok v : Result<'a, 'e> = Choice1Of2 v

  /// Indicates error as Choice2Of2
  let inline Error e : Result<'a, 'e> = Choice2Of2 e
  
  type Input = 
  | BookTitle of string
  | ISBN of string

  type Output = {
    isbn : string
    jetPrice : string
    jetUrl : string
    amazonPrice : string
    amazonUrl : string
  }

  let aiToken = new AIConfiguration("6adee404ca74426e9476a9e56acd32f9", SupportedLanguage.English);
  let apiAi = new ApiAi(aiToken)

  let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None

  let scarpingRun (input:Input) : Output = 
    match input with
    | BookTitle title -> url (sprintf "https://jet.com/search?term=%s" <| title.Replace(" ", "%20"))
    | ISBN isbn -> url (sprintf "https://jet.com/search?term=%s" <| isbn)
    
    click ".products-right"
    let jetPrice = read ".formatted-value"
    let isbn = elementWithText ".h5" "ISBN13" |> read
    let isbnString = isbn.Split([|' ';'\n'|]).[1]
    let jetUrl = currentUrl ()
    url <| "https://www.amazon.com/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords=" + isbnString
    click ".s-access-image.cfMarker"
    let amazonPrice = (elements ".a-size-medium.a-color-price").[0] |> read
    let amazonUrl = currentUrl ()
    {
      isbn = isbnString
      jetPrice = jetPrice
      jetUrl = jetUrl
      amazonPrice = amazonPrice
      amazonUrl = amazonUrl       
    }


  let getScrapingResult (input:Input) : Output = 
    start chrome
    let res = scarpingRun(input)
    quit()
    res
    

  let ProcessMessage (input:string) : Message = 
    match input with    
    | Regex @"look up book (.*)" [ bookTitle ] ->
        let res = getScrapingResult <| BookTitle bookTitle
        let msg = sprintf "Price on jet: %s Price on amazon:%s" res.jetPrice res.amazonPrice
        Message.Text(msg, None, None)
    | Regex @"look up isbn (.*)" [ isbn ] ->
        let res = getScrapingResult <| ISBN isbn
        let msg = sprintf "Price on jet: %s Price on amazon:%s" res.jetPrice res.amazonPrice
        Message.Text(msg, None, None)
    | _ -> Message.Text(apiAi.TextRequest(input).Result.Fulfillment.Speech, None, None)

  let ProcessMessage2 (input:string) : Message = 
//    match input with    
//    | Regex @"look up book (.*)" [ bookTitle ] ->
//        let isbn = getISBN bookTitle
//
//        Message.Text(isbn, None, None)
//    | _ -> 
    Message.Text(apiAi.TextRequest(input).Result.Fulfillment.Speech, None, None)

