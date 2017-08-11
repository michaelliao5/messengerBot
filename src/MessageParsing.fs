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
  open System.Collections.Generic
  open System.Threading
  open System.Globalization

   ///
  let printfnT s = 
    printfn "[%s] %s" (DateTime.Now.ToString()) s

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

  let bookMap = new Dictionary<string, Output>()
  bookMap.Add("the death and life of great american cities", {
    isbn = "9780679741954"
    jetPrice = "10.07" 
    jetUrl = "https://jet.com/product/The-Death-and-Life-of-Great-American-Cities/4878efb2858243e8b393dd260a43e33c"
    amazonPrice = "11.52"
    amazonUrl = "https://www.amazon.com/Death-Life-Great-American-Cities/dp/067974195X"
    })
  
  bookMap.Add("the hunger games", {
    isbn = "9780439023528"
    jetPrice = "8.05"
    jetUrl = "https://jet.com/product/The-Hunger-Games/034c787a86d146c0a0451d4cd4456c38"
    amazonPrice = "8.56"
    amazonUrl = "https://www.amazon.com/Hunger-Games-Book-1/dp/0439023521/ref=sr_1_1?s=books&ie=UTF8&qid=1502385081&sr=1-1&keywords=hunger+games"
  })

  bookMap.Add("new moon", {
    isbn = "9780316024969"
    jetPrice = "9.88"
    jetUrl = "https://jet.com/product/New-Moon-The-Twilight-Saga-Book-2/ca9a1636ee1440c5b36d0cd9e9662df0"
    amazonPrice = "9.87"
    amazonUrl = "https://www.amazon.com/Moon-Twilight-Saga-Stephenie-Meyer/dp/0316024961/ref=sr_1_1?s=books&ie=UTF8&qid=1502385397&sr=1-1&keywords=9780316024969"
  })

  bookMap.Add("divergent", {
    isbn = "9780062024022"
    jetPrice = "10.58"
    jetUrl = "https://jet.com/product/Divergent/a945f01479e34d51b2899e1187ac61c9"
    amazonPrice = "10.58"
    amazonUrl = "https://www.amazon.com/Divergent-Veronica-Roth/dp/0062024027/ref=sr_1_1?s=books&ie=UTF8&qid=1502385710&sr=1-1&keywords=9780062024022"
  })

  bookMap.Add("twilight", {
    isbn = "9780316015844"
    jetPrice = ""
    jetUrl = ""
    amazonPrice = "9.52"
    amazonUrl = "https://www.amazon.com/Twilight-Saga-Book-1/dp/0316015849/ref=sr_1_1?s=books&ie=UTF8&qid=1502385997&sr=1-1&keywords=twilight"
  })

  let aiToken = new AIConfiguration("6adee404ca74426e9476a9e56acd32f9", SupportedLanguage.English);
  let apiAi = new ApiAi(aiToken)

  let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None

  let scarpingRun (input:Input) : Output = 
    printfnT "Navigating to jet.com . . ."
    match input with
    | BookTitle title -> url (sprintf "https://jet.com/search?term=%s" <| title.Replace(" ", "%20"))
    | ISBN isbn -> url (sprintf "https://jet.com/search?term=%s" <| isbn)
    printfnT "Analyzing the cheapest product . . ."
    printfnT "Attempting to click product. . ."
    click ".products-right"
    let jetPrice = read ".formatted-value"
    printfnT "Fetching ISBN-13 . . ."
    let isbn = elementWithText ".h5" "ISBN13" |> read
    printfnT "Fetched ISBN-13, fetching the price of the book . . ."
    let isbnString = isbn.Split([|' ';'\n'|]).[1]
    printfnT "Navigating to amazon . . ."
    let jetUrl = currentUrl ()
    url <| "https://www.amazon.com/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords=" + isbnString
    printfnT "Examining amazon results. . . "
    printfnT "Looking if captcha is present . . ."
    click ".s-access-image.cfMarker"
    let amazonPrice = (elements ".a-size-medium.a-color-price").[0] |> read
    printfnT "Succesfully fetched price from Amazon. . ."
    let amazonUrl = currentUrl ()
    {
      isbn = isbnString
      jetPrice = jetPrice
      jetUrl = jetUrl
      amazonPrice = amazonPrice
      amazonUrl = amazonUrl       
    }



  let getScrapingResult (input:Input) : Output = 
    printfnT "Starting to scrape . . ."
    printfnT "Starting chrome . . ."
    start chrome    
    printfnT "Navigating in chrome . . ."
    let res = scarpingRun(input)
    quit()
    res
    

  let ProcessMessage (input:string) : Message =     
    match input with    
    | Regex @"look up book (.*)" [ bookTitle ] ->
        Thread.Sleep(3000)
        //let res = getScrapingResult <| BookTitle bookTitle
        let key = bookTitle.ToLowerInvariant()
        if bookMap.ContainsKey key then
          let res = bookMap.[key]
          if res.jetPrice |> String.IsNullOrWhiteSpace then                   
            let msg = sprintf @"Item is missing on Jet, but can be found on Amazon
            Price on Amazon:%s
            Here is the link: %s" 
                                res.amazonPrice res.amazonUrl            
            Message.Text(msg, None, None)
          else 
            let jp = Double.Parse res.jetPrice
            let ap = Double.Parse res.amazonPrice
            match ap - jp with
            | x when x < 0.0 -> 
              let msg = sprintf "Jet Price: %s\r\nAmazon Price:%s\r\nAmazon is cheaper!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.amazonUrl        
              Message.Text(msg, None, None)
            | x when x > 0.0 ->
              let msg = sprintf "Jet Price: %s \r\nAmazon Price:%s\r\nJet is cheaper!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.jetUrl        
              Message.Text(msg, None, None)
            | _ -> 
              let msg = sprintf "Jet Price: %s \r\nAmazon Price:%s\r\nThey are the same price but Jet is cheaper if you buy multiple!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.jetUrl        
              Message.Text(msg, None, None)
        else
          let msg = sprintf "Cannot find the book, please try another book!"
          Message.Text(msg, None, None)
//    | Regex @"look up isbn (.*)" [ isbn ] ->
//        let res = getScrapingResult <| ISBN isbn
//        let msg = sprintf "Price on jet: %s Price on amazon:%s" res.jetPrice res.amazonPrice
//        Message.Text(msg, None, None)
    | _ -> Message.Text(apiAi.TextRequest(input).Result.Fulfillment.Speech, None, None)

  let ProcessMessage2 (input:string) : Message =     
    match input with    
    | Regex @"look up book (.*)" [ bookTitle ] -> 
        printfnT (sprintf "Processing book title %s" bookTitle)
        let res = getScrapingResult <| BookTitle bookTitle
        if res.jetPrice |> String.IsNullOrWhiteSpace then                   
            let msg = sprintf @"Item is missing on Jet, but can be found on Amazon
            Price on Amazon:%s
            Here is the link: %s" 
                                res.amazonPrice res.amazonUrl            
            Message.Text(msg, None, None)
          else 
            let jp = Decimal.Parse(res.jetPrice.Replace(" ", String.Empty), NumberStyles.Currency);
            let ap = Decimal.Parse(res.amazonPrice.Replace(" ", String.Empty), NumberStyles.Currency);
            match ap - jp with
            | x when x < 0.0m -> 
              let msg = sprintf "Jet Price: %s\r\nAmazon Price:%s\r\nAmazon is cheaper!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.amazonUrl        
              Message.Text(msg, None, None)
            | x when x > 0m ->
              let msg = sprintf "Jet Price: %s \r\nAmazon Price:%s\r\nJet is cheaper!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.jetUrl        
              Message.Text(msg, None, None)
            | _ -> 
              let msg = sprintf "Jet Price: %s \r\nAmazon Price:%s\r\nThey are the same price but Jet is cheaper if you buy multiple!\r\nHere is the link %s"
                                  res.jetPrice res.amazonPrice res.jetUrl        
              Message.Text(msg, None, None)
//    | Regex @"look up isbn (.*)" [ isbn ] ->
//        let res = getScrapingResult <| ISBN isbn
//        let msg = sprintf "Price on jet: %s Price on amazon:%s" res.jetPrice res.amazonPrice
//        Message.Text(msg, None, None)
    | _ -> Message.Text(apiAi.TextRequest(input).Result.Fulfillment.Speech, None, None)




