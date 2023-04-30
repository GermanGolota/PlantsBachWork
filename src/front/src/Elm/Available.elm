module Available exposing (Available, availableDecoder)

import Json.Decode as D
import Multiselect as Multiselect


availableDecoder : D.Decoder Available
availableDecoder =
    D.map3 Available
        ((D.field "regions" <| D.list D.string)
            |> dictDecoder "region"
        )
        ((D.field "soils" <| D.list D.string)
            |> dictDecoder "soil"
        )
        (D.field "families" (D.list D.string)
            |> dictDecoder "family"
        )


dictDecoder : String -> D.Decoder (List String) -> D.Decoder Multiselect.Model
dictDecoder tag base =
    D.map (convertDict tag) base


convertDict : String -> List String -> Multiselect.Model
convertDict tag items =
    Multiselect.initModel (List.map (\item -> Tuple.pair item item) items) tag Multiselect.Show


type alias Available =
    { regions : Multiselect.Model
    , soils : Multiselect.Model
    , families : Multiselect.Model
    }
