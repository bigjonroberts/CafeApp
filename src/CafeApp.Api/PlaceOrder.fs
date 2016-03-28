module PlaceOrder
open FSharp.Data
open Queries
open Commands
open Domain
open CommandHandlers

[<Literal>]
let PlaceOrderJson = """{
  "placeOrder" : {
    "tabId" : "2a964d85-f503-40a1-8014-2c8ee5ac4a49",
    "foodMenuNumbers" : [8,9],
    "drinkMenuNumbers" : [10,11]
  }
}"""
type PlaceOrderReq = JsonProvider<PlaceOrderJson>

let (|PlaceOrderRequest|_|) payload =
  try
    PlaceOrderReq.Parse(payload).PlaceOrder
    |> Some
  with
  | ex -> None

let validateTabId getTableByTabId (tabId, drinks, foods)  =
  match getTableByTabId tabId with
  | Some _ -> Choice1Of2 (drinks,foods)
  | _ -> Choice2Of2 "Invalid Tab Id"

let validatePlaceOrder queries (c : PlaceOrderReq.PlaceOrder) = async {
  let! table = queries.GetTableByTabId c.TabId
  match table with
  | Some _ ->
      let! foodItems =
        queries.GetFoodsByMenuNumbers c.FoodMenuNumbers
      let! drinksItems =
        queries.GetDrinksByMenuNumbers c.DrinkMenuNumbers
      let isEmptyOrder foods drinks =
        List.isEmpty foods && List.isEmpty drinks
      match foodItems, drinksItems with
      | Choice1Of2 foods, Choice1Of2 drinks ->
          if isEmptyOrder foods drinks then
            let msg = "Order Should Contain atleast 1 food or drinks"
            return Choice2Of2 msg
          else
            return Choice1Of2 (c.TabId, drinks, foods)
      | Choice2Of2 fkeys, Choice2Of2 dkeys ->
          let msg =
            sprintf "Invalid Food Keys : %A, Invalid Drinks Keys %A"
              fkeys dkeys
          return Choice2Of2 msg
      | Choice2Of2 keys, _ ->
        let msg = sprintf "Invalid Food Keys : %A" keys
        return Choice2Of2 msg
      | _, Choice2Of2 keys ->
          let msg = sprintf "Invalid Drinks Keys : %A" keys
          return Choice2Of2 msg
    | _ -> return Choice2Of2 "Invalid Tab Id"
  }

let toPlaceOrderCommand (tabId, drinks, foods) =
  {
    TabId = tabId
    FoodItems = foods
    DrinksItems = drinks
  }
  |> PlaceOrder

let placeOrderCommander validationQueries = {
  Validate = validatePlaceOrder validationQueries
  ToCommand = toPlaceOrderCommand
}