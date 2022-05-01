module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed, imageIdToUrl)
import Html exposing (Html, div, text)
import Html.Attributes exposing (href, style)
import Http
import ImageList as ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (searchLink, viewNav)
import Utils exposing (fillParent, flex, flex1, formatPrice, largeFont, mediumFont, mediumMargin, smallMargin, textCenter)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant PlantView


type alias PlantView =
    { id : Int
    , plant : WebData PlantModel
    }



--update


type alias PlantModel =
    { name : String
    , description : String
    , price : Float
    , soil : String
    , regions : List String
    , group : String

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


type Msg
    = GotPlant (Result Http.Error PlantModel)
    | Images ImageList.Msg


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            let
                authed newModel =
                    Authorized auth newModel

                authedPlant plantView =
                    authed <| Plant plantView
            in
            case ( msg, model ) of
                ( GotPlant (Ok res), Plant p ) ->
                    ( authedPlant { p | plant = Loaded res }, Cmd.none )

                ( GotPlant (Err res), Plant p ) ->
                    ( authedPlant { p | plant = Error }, Cmd.none )

                ( Images img, Plant p ) ->
                    case p.plant of
                        Loaded pl ->
                            ( authedPlant { p | plant = Loaded { pl | images = ImageList.update img pl.images } }, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( _, _ ) ->
                    ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder token)
    in
    getAuthed token (PlantE plantId) expect Nothing


plantDecoder : String -> D.Decoder PlantModel
plantDecoder token =
    D.succeed PlantModel
        |> required "plantName" D.string
        |> required "description" D.string
        |> required "price" D.float
        |> required "soilName" D.string
        |> required "regions" (D.list D.string)
        |> required "groupName" D.string
        |> custom createdDecoder
        |> required "sellerName" D.string
        |> required "sellerPhone" D.string
        |> custom (credsDecoder "seller")
        |> custom (credsDecoder "careTaker")
        |> custom (imagesDecoder token)


imagesDecoder : String -> D.Decoder ImageList.Model
imagesDecoder token =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.field "images" (D.list D.int))


imageIdsToModel : String -> List Int -> ImageList.Model
imageIdsToModel token ids =
    let
        baseList =
            List.map (\id -> ( id, imageIdToUrl token id )) ids
    in
    ImageList.fromDict <| Dict.fromList baseList


credsDecoder : String -> D.Decoder PersonCreds
credsDecoder person =
    let
        combine str =
            person ++ str
    in
    D.succeed PersonCreds
        |> required (combine "Cared") D.int
        |> required (combine "Sold") D.int
        |> required (combine "Instructions") D.int


createdDecoder : D.Decoder String
createdDecoder =
    let
        combine date humanDate =
            date ++ "(" ++ humanDate ++ ")"
    in
    D.map2
        combine
        (D.field "createdDate" D.string)
        (D.field "createdHumanDate" D.string)



--view


view : Model -> Html Msg
view model =
    viewNav model (Just searchLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage _ page =
    case page of
        NoPlant ->
            div [] [ text "Please select a plant", Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href "/search" ] ] [ text "Return to search" ] ]

        Plant plant ->
            let
                plantView =
                    viewPlant plant.id
            in
            viewWebdata plant.plant plantView


viewPlant : Int -> PlantModel -> Html Msg
viewPlant id plant =
    let
        filled args =
            fillParent ++ args

        largeCentered =
            [ largeFont, textCenter ]
    in
    div (filled [ flex, Flex.row ])
        [ div [ flex, Flex.col, flex1 ]
            [ div largeCentered [ text plant.name ]
            , Html.map Images <| ImageList.view plant.images
            , viewPlantStat "Caretaker Credentials" (credsToString plant.caretakerCreds)
            , viewPlantStat "Seller Credentials" (credsToString plant.sellerCreds)
            , viewPlantStat "Seller Nama" plant.sellerName
            , viewPlantStat "Seller Phone" plant.sellerPhone
            ]
        , div [ flex, Flex.col, flex1 ]
            [ div largeCentered [ text "Description" ]
            , div [] [ text plant.description ]
            , div largeCentered [ text <| formatPrice plant.price ]
            , viewPlantStat "Soil" plant.soil
            , viewPlantStat "Regions" (String.join ", " plant.regions)
            , viewPlantStat "Group" plant.group
            , viewPlantStat "Age" plant.created
            , interactionButtons id
            ]
        ]


credsToString : PersonCreds -> String
credsToString creds =
    String.fromInt creds.cared ++ " cared, " ++ String.fromInt creds.sold ++ " sold, " ++ String.fromInt creds.instructions ++ " instructions published"


viewPlantStat desc value =
    flexRowGap (div [ mediumMargin, mediumFont ] [ text desc ]) (div [ mediumMargin, mediumFont ] [ text value ])


flexRowGap left right =
    div [ flex, Flex.row, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ left
        , right
        ]


interactionButtons id =
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/search", largeFont ] ] [ text "Back" ]
        , Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/order/" ++ String.fromInt id, largeFont ] ] [ text "Order" ]
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialModel =
            decodeInitial flags

        cmds authResp =
            case initialModel of
                Plant p ->
                    getPlantCommand authResp.token p.id

                NoPlant ->
                    Cmd.none
    in
    initBase
        [ Producer, Consumer, Manager ]
        initialModel
        cmds
        resp


decodeInitial : D.Value -> View
decodeInitial flags =
    case decodePlantId flags of
        Err _ ->
            NoPlant

        Ok plantId ->
            case String.toInt plantId of
                Just plantNumber ->
                    Plant (PlantView plantNumber Loading)

                Nothing ->
                    NoPlant


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
