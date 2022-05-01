module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Endpoints exposing (Endpoint(..), getAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (href)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (searchLink, viewNav)
import Utils exposing (smallMargin)
import Webdata exposing (WebData(..))



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
    }


type alias PersonCreds =
    { sold : Int
    , cared : Int
    , instructions : Int
    }


type Msg
    = GotPlant (Result Http.Error PlantModel)


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

                ( _, _ ) ->
                    ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant plantDecoder
    in
    getAuthed token (PlantE plantId) expect Nothing


plantDecoder : D.Decoder PlantModel
plantDecoder =
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
            div [] []



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
