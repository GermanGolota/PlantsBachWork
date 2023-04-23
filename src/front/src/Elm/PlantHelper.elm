module PlantHelper exposing (PlantModel, plantDecoder, viewDesc, viewPlantBase, viewPlantLeft)

import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (imagesDecoder)
import Html exposing (Html, div, text)
import Html.Attributes exposing (style)
import ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, hardcoded, requiredAt)
import Utils exposing (createdDecoder, existsDecoder, fillParent, flex, flex1, formatPrice, largeCentered, largeFont, mediumFont, mediumMargin, smallMargin)


type alias PlantModel =
    { name : String
    , description : String
    , price : Float
    , soils : List String
    , regions : List String
    , families : List String

    --{createdDate}({createdHumanDate})
    , created : String
    , sellerName : String
    , sellerPhone : String
    , sellerCreds : PersonCreds
    , caretakerCreds : PersonCreds
    , images : ImageList.Model
    }


type alias PersonCreds =
    { sold : Int
    , cared : Int
    , instructions : Int
    }


plantDecoder : Maybe Float -> String -> D.Decoder (Maybe PlantModel)
plantDecoder priceOverride token =
    existsDecoder (plantDecoderBase priceOverride token)


plantDecoderBase : Maybe Float -> String -> D.Decoder PlantModel
plantDecoderBase priceOverride token =
    let
        requiredItem name =
            requiredAt [ "item", name ]

        priceDecoder =
            case priceOverride of
                Just price ->
                    hardcoded price

                Nothing ->
                    requiredItem "price" D.float
    in
    D.succeed PlantModel
        |> requiredItem "plantName" D.string
        |> requiredItem "description" D.string
        |> priceDecoder
        |> requiredItem "soilNames" (D.list D.string)
        |> requiredItem "regionNames" (D.list D.string)
        |> requiredItem "familyNames" (D.list D.string)
        |> custom createdDecoder
        |> requiredItem "sellerName" D.string
        |> requiredItem "sellerPhone" D.string
        |> custom (credsDecoder "seller")
        |> custom (credsDecoder "careTaker")
        |> custom (imagesDecoder token [ "item", "images" ])


credsDecoder : String -> D.Decoder PersonCreds
credsDecoder person =
    let
        combine str =
            person ++ str

        requiredItem val =
            requiredAt [ "item", val ]
    in
    D.succeed PersonCreds
        |> requiredItem (combine "Cared") D.int
        |> requiredItem (combine "Sold") D.int
        |> requiredItem (combine "Instructions") D.int


viewPlantBase : Bool -> (String -> msg) -> (ImageList.Msg -> msg) -> Html msg -> PlantModel -> Html msg
viewPlantBase priceEditable eventConverter imgConverter btns plant =
    let
        filled args =
            fillParent ++ args

        desc =
            viewDesc priceEditable eventConverter plant
    in
    div (filled [ flex, Flex.row ])
        [ viewPlantLeft imgConverter plant
        , div [ flex, Flex.col, flex1 ]
            (desc
                ++ [ viewPlantStat "Soils" (String.join ", " plant.soils)
                   , viewPlantStat "Regions" (String.join ", " plant.regions)
                   , viewPlantStat "Families" (String.join ", " plant.families)
                   , viewPlantStat "Age" plant.created
                   , btns
                   ]
            )
        ]


viewDesc : Bool -> (String -> msg) -> PlantModel -> List (Html msg)
viewDesc priceEditable eventConverter plant =
    let
        priceView =
            if priceEditable then
                div [ flex, Flex.row, largeFont ]
                    [ Input.number
                        [ Input.onInput eventConverter
                        , Input.attrs [ style "flex" "7", style "text-align" "right" ]
                        ]
                    , div [ flex1, smallMargin ] [ text " ₴" ]
                    ]

            else
                div largeCentered [ text <| formatPrice plant.price ]
    in
    [ div largeCentered [ text "Description" ]
    , div [] [ text plant.description ]
    , priceView
    ]


viewPlantLeft : (ImageList.Msg -> msg) -> PlantModel -> Html msg
viewPlantLeft convert plant =
    div [ flex, Flex.col, flex1 ]
        [ div largeCentered [ text plant.name ]
        , Html.map convert <| ImageList.view plant.images
        , viewPlantStat "Caretaker Credentials" (credsToString plant.caretakerCreds)
        , viewPlantStat "Seller Credentials" (credsToString plant.sellerCreds)
        , viewPlantStat "Seller Name" plant.sellerName
        , viewPlantStat "Seller Phone" plant.sellerPhone
        ]


credsToString : PersonCreds -> String
credsToString creds =
    String.fromInt creds.cared ++ " cared, " ++ String.fromInt creds.sold ++ " sold, " ++ String.fromInt creds.instructions ++ " instructions published"


viewPlantStat : String -> String -> Html msg
viewPlantStat desc value =
    flexRowGap (div [ mediumMargin, mediumFont ] [ text desc ]) (div [ mediumMargin, mediumFont ] [ text value ])


flexRowGap left right =
    div [ flex, Flex.row, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ left
        , right
        ]
