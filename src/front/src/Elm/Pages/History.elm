module Pages.History exposing (..)

import Bootstrap.Accordion as Accordion
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.ListGroup as ListGroup
import Bootstrap.Text as Text
import Bootstrap.Utilities.Flex as Flex
import Dict exposing (Dict)
import Endpoints exposing (getAuthedQuery)
import Html exposing (Html, div, text)
import Http
import Json.Decode as D exposing (errorToString, field)
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import JsonViewer exposing (initJsonTree, updateJsonTree, viewJsonTree)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, updateBase)
import NavBar exposing (plantsLink, viewNav)
import Utils exposing (buildQuery, fillParent, humanizePascalCase)
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
    List ( AggregateSnapshot, Accordion.State )


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
    { metadata : EventMetadata
    , payload : D.Value
    }


type alias EventMetadata =
    { name : String
    , id : String
    , fullMetadata : D.Value
    }


type alias AggregateData =
    { metadata : D.Value
    , payload : D.Value
    }


type alias CommandData =
    { isLocal : Bool
    , metadata : CommandMetadata
    , payload : D.Value
    , payloadView : JsonViewer.Model
    }


type alias CommandMetadata =
    { userName : String
    , name : String
    , id : String
    , fullMetadata : D.Value
    }



--update


type LocalMsg
    = NoOp
    | GotAggregate (Result Http.Error History)
    | AccordionMsg AggregateSnapshot Accordion.State
    | CommandDataJson AggregateSnapshot JsonViewer.Msg


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

                        AccordionMsg aggregate state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        mapSnapshot snapshot oldState =
                                            if snapshot == aggregate then
                                                ( snapshot, state )

                                            else
                                                ( snapshot, oldState )
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded <| List.map (\( k, v ) -> mapSnapshot k v) history }, Cmd.none )

                                _ ->
                                    noOp

                        GotAggregate (Err err) ->
                            ( authed <| Valid <| { viewModel | history = Debug.log (errorToString err) Error }, Cmd.none )

                        GotAggregate (Ok history) ->
                            ( authed <| Valid <| { viewModel | history = Loaded history }, Cmd.none )

                        CommandDataJson aggregate json ->
                            case viewModel.history of
                                Loaded history ->
                                    ( authed <| Valid <| { viewModel | history = Loaded <| List.map (mapJsonSnapshot aggregate json) history }, Cmd.none )

                                _ ->
                                    noOp

        _ ->
            ( m, Cmd.none )


mapJsonSnapshot : AggregateSnapshot -> JsonViewer.Msg -> ( AggregateSnapshot, Accordion.State ) -> ( AggregateSnapshot, Accordion.State )
mapJsonSnapshot changed json ( snapshot, state ) =
    if snapshot == changed then
        ( { snapshot | lastCommand = updateJson snapshot.lastCommand json }, state )

    else
        ( snapshot, state )


updateJson : CommandData -> JsonViewer.Msg -> CommandData
updateJson command json =
    { command | payloadView = updateJsonTree json command.payloadView }


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
    D.field "snapshots" (D.list snapshotDecoder)
        |> D.andThen (\d -> D.succeed <| snapshotMapper d)


snapshotMapper : List AggregateSnapshot -> History
snapshotMapper snapshots =
    List.map (\v -> ( v, Accordion.initialState )) snapshots



--Dict.fromList <| List.map (\v -> ( v, Accordion.initialState ) snapshots


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
        |> required "metadata" eventMetadataDecoder
        |> required "payload" D.value


eventMetadataDecoder : D.Decoder EventMetadata
eventMetadataDecoder =
    D.succeed EventMetadata
        |> required "name" D.string
        |> required "id" D.string
        |> custom D.value


commandDecoder : D.Decoder CommandData
commandDecoder =
    D.map3 buildCommandData (field "isLocal" D.bool) (field "metadata" commandMetadataDecoder) (field "payload" D.value)


buildCommandData local meta payload =
    CommandData local meta payload (initJsonTree payload)


commandMetadataDecoder : D.Decoder CommandMetadata
commandMetadataDecoder =
    D.succeed CommandMetadata
        |> required "userName" D.string
        |> required "name" D.string
        |> required "id" D.string
        |> custom D.value



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
    div fillParent [ ListGroup.ul (List.map (\( snapshot, state ) -> ListGroup.li [] [ viewSnapshot snapshot state ]) history) ]


viewSnapshot : AggregateSnapshot -> Accordion.State -> Html LocalMsg
viewSnapshot snapshot state =
    let
        commandMeta =
            snapshot.lastCommand.metadata
    in
    div []
        [ text ("\"" ++ commandMeta.userName ++ "\"" ++ " executed " ++ "\"" ++ humanizePascalCase commandMeta.name ++ "\"" ++ " " ++ snapshot.displayTime)
        , Accordion.config (AccordionMsg snapshot)
            |> Accordion.withAnimation
            |> Accordion.cards
                ([ viewCommandData snapshot snapshot.lastCommand ]
                    ++ List.map viewSnapshotEvent snapshot.events
                )
            |> Accordion.view state
        ]


viewCommandData : AggregateSnapshot -> CommandData -> Accordion.Card LocalMsg
viewCommandData snapshot command =
    Accordion.card
        { id = command.metadata.id
        , options = [ Card.outlineSuccess, Card.align Text.alignXsCenter ]
        , header =
            Accordion.header [] <| Accordion.toggle [] [ text <| "Data" ]
        , blocks =
            [ Accordion.block []
                [ Block.custom <| (viewJsonTree command.payloadView |> Html.map (CommandDataJson snapshot)) ]
            ]
        }


viewSnapshotEvent : EventData -> Accordion.Card msg
viewSnapshotEvent event =
    Accordion.card
        { id = event.metadata.id
        , options = []
        , header =
            Accordion.header [] <| Accordion.toggle [] [ text <| humanizePascalCase event.metadata.name ]
        , blocks =
            viewEvent event
        }


viewEvent event =
    [ Accordion.block []
        [ Block.text [] [ text event.metadata.name ] ]
    ]


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
    case model of
        Authorized _ a ->
            case a of
                Valid v ->
                    case v.history of
                        Loaded history ->
                            history
                                |> List.map (\( agg, state ) -> Accordion.subscriptions state (\st -> Main <| AccordionMsg agg st))
                                |> Sub.batch

                        _ ->
                            Sub.none

                _ ->
                    Sub.none

        _ ->
            Sub.none



--Accordion.subscriptions model.accordionState AccordionMsg
--Sub.batch [
--  List.map (\val -> Accordion.subscriptions state (AccordionMsg agg) state)
--]


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
