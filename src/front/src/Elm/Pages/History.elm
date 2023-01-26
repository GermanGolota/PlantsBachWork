module Pages.History exposing (..)

import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (getAuthedQuery)
import Html exposing (Html, div, text)
import Http
import Json.Decode as D exposing (errorToString)
import Json.Decode.Pipeline exposing (required, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, updateBase)
import NavBar exposing (plantsLink, viewNav)
import Utils exposing (buildQuery)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Valid ViewValue
    | Invalid


type alias ViewValue =
    { aggregate : AggregateDescription
    , history : WebData History
    }


type alias AggregateDescription =
    { name : String
    , id : String
    }


type alias History =
    List AggregateSnapshot


type alias AggregateSnapshot =
    { time : String
    , displayTime : String
    , lastCommand : CommandData
    , events : List EventData
    , aggregate : AggregateData
    , related : List RelatedAggregate
    }


type alias RelatedAggregate =
    { name : String
    , role : String
    , id : String
    }


type alias EventData =
    { metadata : D.Value
    , payload : D.Value
    }


type alias AggregateData =
    { metadata : D.Value
    , payload : D.Value
    }


type alias CommandData =
    { isLocal : Bool
    , metadata : D.Value
    , payload : D.Value
    }



--update


type LocalMsg
    = NoOp
    | GotAggregate (Result Http.Error History)


type alias Msg =
    MsgBase LocalMsg


update : LocalMsg -> Model -> ( Model, Cmd Msg )
update msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed =
                    Authorized auth

                updateModel newModel =
                    ( authed <| newModel, Cmd.none )
            in
            case model of
                Invalid ->
                    noOp

                Valid viewModel ->
                    case msg of
                        NoOp ->
                            noOp

                        GotAggregate (Err err) ->
                            ( authed <| Valid <| { viewModel | history = Debug.log (errorToString err) Error }, Cmd.none )

                        GotAggregate (Ok history) ->
                            ( authed <| Valid <| { viewModel | history = Loaded history }, Cmd.none )

        _ ->
            ( m, Cmd.none )


errorToString : Http.Error -> String
errorToString error =
    case error of
        Http.BadUrl url ->
            "The URL " ++ url ++ " was invalid"

        Http.Timeout ->
            "Unable to reach the server, try again"

        Http.NetworkError ->
            "Unable to reach the server, check your network connection"

        Http.BadStatus 500 ->
            "The server had a problem, try again later"

        Http.BadStatus 400 ->
            "Verify your information and try again"

        Http.BadStatus _ ->
            "Unknown error"

        Http.BadBody errorMessage ->
            errorMessage



--commands


loadHistoryCmd : String -> AggregateDescription -> Cmd Msg
loadHistoryCmd token aggregate =
    let
        expect =
            Http.expectJson GotAggregate historyDecoder

        query =
            buildQuery [ ( "name", aggregate.name ), ( "id", aggregate.id ) ]
    in
    getAuthedQuery query token Endpoints.History expect Nothing |> mapCmd


historyDecoder : D.Decoder History
historyDecoder =
    D.field "snapshots" <| D.list snapshotDecoder


snapshotDecoder : D.Decoder AggregateSnapshot
snapshotDecoder =
    let
        requiredItem name =
            requiredAt [ "snapshot", name ]
    in
    D.succeed AggregateSnapshot
        |> requiredItem "time" D.string
        |> required "humanTime" D.string
        |> requiredItem "lastCommand" commandDecoder
        |> requiredItem "events" (D.list eventDecoder)
        |> requiredItem "aggregate" aggregateDecoder
        |> requiredItem "related" (D.list relatedDecoder)


relatedDecoder : D.Decoder RelatedAggregate
relatedDecoder =
    D.succeed RelatedAggregate
        |> required "name" D.string
        |> required "role" D.string
        |> required "id" D.string


aggregateDecoder : D.Decoder AggregateData
aggregateDecoder =
    D.succeed AggregateData
        |> required "metadata" D.value
        |> required "payload" D.value


eventDecoder : D.Decoder EventData
eventDecoder =
    D.succeed EventData
        |> required "metadata" D.value
        |> required "payload" D.value


commandDecoder : D.Decoder CommandData
commandDecoder =
    D.succeed CommandData
        |> required "isLocal" D.bool
        |> required "metadata" D.value
        |> required "payload" D.value



--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        Valid agg ->
            viewWebdata agg.history viewHistory |> Html.map Main

        Invalid ->
            div [] [ text "Failed to load aggregate" ]


viewHistory : History -> Html LocalMsg
viewHistory history =
    div [] [ text <| "Viewing history for " ++ (String.fromInt <| List.length history) ++ " records" ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        getAggregate =
            D.decodeValue (D.map2 AggregateDescription (D.field "name" D.string) (D.field "id" D.string)) flags

        initialState =
            case getAggregate of
                Ok agg ->
                    Valid { aggregate = agg, history = Loading }

                Err err ->
                    Invalid

        initialCmd res =
            case initialState of
                Valid agg ->
                    loadHistoryCmd res.token agg.aggregate

                Invalid ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initialState initialCmd resp


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
