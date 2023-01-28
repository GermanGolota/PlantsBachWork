port module Pages.History exposing (..)

import Bootstrap.Accordion as Accordion exposing (State(..))
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Text as Text
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (getAuthedQuery)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import JsonViewer exposing (initJsonTree, initJsonTreeCollapsed, updateJsonTree, viewJsonTree)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, updateBase)
import NavBar exposing (plantsLink, viewNav)
import Utils exposing (buildQuery, fillParent, flex, humanizePascalCase, largeCentered, mediumMargin)
import Webdata exposing (WebData(..), viewWebdata)



-- ports


port resizeAggregates : () -> Cmd msg



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
    { snapshots : List ( AggregateSnapshot, Accordion.State )
    , aggregateView : Accordion.State
    }


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
    , payload : JsonViewer.Model
    }


type alias EventMetadata =
    { name : String
    , id : String
    , fullMetadata : D.Value
    }


type alias AggregateData =
    { metadata : D.Value
    , payload : JsonViewer.Model
    }


type alias CommandData =
    { isLocal : Bool
    , metadata : CommandMetadata
    , payload : JsonViewer.Model
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
    | AccordionSnapshotMsg AggregateSnapshot Accordion.State
    | AccordionAggregateMsg Accordion.State
    | CommandDataJson AggregateSnapshot JsonViewer.Msg
    | EventDataJson AggregateSnapshot EventData JsonViewer.Msg
    | AggregateDataJson AggregateSnapshot JsonViewer.Msg


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

                        AccordionSnapshotMsg aggregate state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        mapSnapshot snapshot oldState =
                                            if snapshot == aggregate then
                                                ( snapshot, state )

                                            else
                                                ( snapshot, oldState )

                                        updateHistory =
                                            { history | snapshots = List.map (\( k, v ) -> mapSnapshot k v) history.snapshots }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, resizeAggregates () )

                                _ ->
                                    noOp

                        AccordionAggregateMsg state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateHistory =
                                            { history | aggregateView = state }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, resizeAggregates () )

                                _ ->
                                    noOp

                        GotAggregate (Err err) ->
                            ( authed <| Valid <| { viewModel | history = Error }, Cmd.none )

                        GotAggregate (Ok history) ->
                            ( authed <| Valid <| { viewModel | history = Loaded history }, Cmd.none )

                        CommandDataJson changed json ->
                            let
                                mapJsonSnapshot ( snapshot, state ) =
                                    if snapshot == changed then
                                        ( { snapshot | lastCommand = updateJsonCommand snapshot.lastCommand json }, state )

                                    else
                                        ( snapshot, state )
                            in
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateHistory =
                                            { history | snapshots = List.map mapJsonSnapshot history.snapshots }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, Cmd.none )

                                _ ->
                                    noOp

                        AggregateDataJson changed json ->
                            let
                                mapJsonSnapshot ( snapshot, state ) =
                                    if snapshot == changed then
                                        ( { snapshot | aggregate = updateJsonAggregate snapshot.aggregate json }, state )

                                    else
                                        ( snapshot, state )
                            in
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateHistory =
                                            { history | snapshots = List.map mapJsonSnapshot history.snapshots }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, Cmd.none )

                                _ ->
                                    noOp

                        EventDataJson changeSnapshot changedEvent message ->
                            let
                                mapEvent event =
                                    if event == changedEvent then
                                        { event | payload = updateJsonTree message event.payload }

                                    else
                                        event

                                mapJsonSnapshot ( snapshot, state ) =
                                    if snapshot == changeSnapshot then
                                        ( { snapshot | events = List.map mapEvent snapshot.events }, state )

                                    else
                                        ( snapshot, state )
                            in
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateHistory =
                                            { history | snapshots = List.map mapJsonSnapshot history.snapshots }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, Cmd.none )

                                _ ->
                                    noOp

        _ ->
            ( m, Cmd.none )


updateJsonCommand : CommandData -> JsonViewer.Msg -> CommandData
updateJsonCommand command json =
    { command | payload = updateJsonTree json command.payload }


updateJsonAggregate : AggregateData -> JsonViewer.Msg -> AggregateData
updateJsonAggregate aggregate json =
    { aggregate | payload = updateJsonTree json aggregate.payload }



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
    History (List.map (\v -> ( v, Accordion.initialState )) snapshots) Accordion.initialState


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
        |> custom (D.map initJsonTreeCollapsed (D.field "payload" D.value))


eventDecoder : D.Decoder EventData
eventDecoder =
    D.succeed EventData
        |> required "metadata" eventMetadataDecoder
        |> custom (D.map initJsonTree (D.field "payload" D.value))


eventMetadataDecoder : D.Decoder EventMetadata
eventMetadataDecoder =
    D.succeed EventMetadata
        |> required "name" D.string
        |> required "id" D.string
        |> custom D.value


commandDecoder : D.Decoder CommandData
commandDecoder =
    D.succeed CommandData
        |> required "isLocal" D.bool
        |> required "metadata" commandMetadataDecoder
        |> custom (D.map initJsonTree (D.field "payload" D.value))


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
    div fillParent
        [ Accordion.config AccordionAggregateMsg
            --|> Accordion.withAnimation
            |> Accordion.cards
                (List.map (\( k, v ) -> viewSnapshot history.aggregateView k v) history.snapshots)
            |> Accordion.view history.aggregateView
        ]


viewSnapshot : Accordion.State -> AggregateSnapshot -> Accordion.State -> Accordion.Card LocalMsg
viewSnapshot baseState snapshot state =
    let
        id =
            snapshot.lastCommand.metadata.id
    in
    Accordion.card
        { id = id
        , options = [ Card.outlineDark, Card.align Text.alignXsCenter ]
        , header =
            Accordion.header [] <| Accordion.toggle [] [ viewSnapshotName snapshot ]
        , blocks =
            [ Accordion.block []
                [ Block.custom <|
                    div []
                        [ div [ flex, Flex.row, mediumMargin ]
                            [ div [ flex, Flex.col, style "width" "50%" ]
                                [ div largeCentered [ text "State" ]
                                , viewJsonTree snapshot.aggregate.payload |> Html.map (AggregateDataJson snapshot)
                                ]
                            , div [ flex, Flex.col, style "width" "50%" ]
                                [ div largeCentered [ text "Command" ]
                                , Accordion.config (AccordionSnapshotMsg snapshot)
                                    -- |> Accordion.withAnimation
                                    |> Accordion.cards
                                        ([ viewCommandData snapshot snapshot.lastCommand ]
                                            ++ List.map (viewSnapshotEvent snapshot) snapshot.events
                                        )
                                    |> Accordion.view state
                                ]
                            ]
                        ]
                ]
            ]
        }


blockStyle : String -> Accordion.State -> List (Html.Attribute msg)
blockStyle id state =
    if Accordion.isOpen id state then
        Debug.log "openAboba" [ style "height" "0px" ]

    else
        Debug.log "NotopenAboba" [ style "height" "100% !important" ]


viewSnapshotName : AggregateSnapshot -> Html msg
viewSnapshotName snapshot =
    let
        isLocal =
            snapshot.lastCommand.isLocal

        textColor =
            if isLocal then
                "text-primary"

            else
                "text-secondary"

        actionName =
            if isLocal then
                " executed "

            else
                " received "
    in
    div (largeCentered ++ [ class textColor ]) [ text ("\"" ++ snapshot.lastCommand.metadata.userName ++ "\"" ++ actionName ++ "\"" ++ humanizePascalCase snapshot.lastCommand.metadata.name ++ "\"" ++ " " ++ snapshot.displayTime) ]


viewCommandData : AggregateSnapshot -> CommandData -> Accordion.Card LocalMsg
viewCommandData snapshot command =
    let
        outline =
            if command.isLocal then
                Card.outlineSuccess

            else
                Card.outlineSecondary
    in
    Accordion.card
        { id = command.metadata.id
        , options = [ outline, Card.align Text.alignXsCenter ]
        , header =
            Accordion.header [] <| Accordion.toggle [] [ text <| "Command Data" ]
        , blocks =
            [ Accordion.block []
                [ Block.custom <| (viewJsonTree command.payload |> Html.map (CommandDataJson snapshot)) ]
            ]
        }


viewSnapshotEvent : AggregateSnapshot -> EventData -> Accordion.Card LocalMsg
viewSnapshotEvent snapshot event =
    Accordion.card
        { id = event.metadata.id
        , options = [ Card.outlineSecondary, Card.align Text.alignXsCenter ]
        , header =
            Accordion.header [] <| Accordion.toggle [] [ text <| ("Event \"" ++ humanizePascalCase event.metadata.name ++ "\"") ]
        , blocks =
            [ Accordion.block []
                [ Block.custom <| (viewJsonTree event.payload |> Html.map (EventDataJson snapshot event)) ]
            ]
        }


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
                            let
                                snapshotSubs =
                                    history.snapshots
                                        |> List.map (\( agg, state ) -> Accordion.subscriptions state (\st -> Main <| AccordionSnapshotMsg agg st))
                            in
                            snapshotSubs
                                ++ [ Accordion.subscriptions history.aggregateView (\st -> Main <| AccordionAggregateMsg st) ]
                                |> Sub.batch

                        _ ->
                            Sub.none

                _ ->
                    Sub.none

        _ ->
            Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
