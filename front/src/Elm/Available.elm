module Available exposing (Available, availableDecoder)

import Dict exposing (Dict)
import Json.Decode as D
import Multiselect as Multiselect


availableDecoder : D.Decoder Available
availableDecoder =
    D.map3 Available
        ((D.field "regions" <| D.dict D.string)
            |> dictDecoder "region"
        )
        ((D.field "soils" <| D.dict D.string)
            |> dictDecoder "soil"
        )
        (D.field "groups" (D.dict D.string)
            |> dictDecoder "group"
        )


dictDecoder : String -> D.Decoder (Dict String String) -> D.Decoder Multiselect.Model
dictDecoder tag base =
    D.map (convertDict tag) base


convertDict : String -> Dict String String -> Multiselect.Model
convertDict tag dict =
    Multiselect.initModel (Dict.toList dict) tag Multiselect.Show


type alias Available =
    { regions : Multiselect.Model
    , soils : Multiselect.Model
    , groups : Multiselect.Model
    }
