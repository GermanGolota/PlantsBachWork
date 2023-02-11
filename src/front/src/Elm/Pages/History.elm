module Pages.History exposing (..)

import Bootstrap.Accordion as Accordion exposing (State(..))
import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form.Checkbox as Checkbox
import Bootstrap.Form.Input as Input
import Bootstrap.ListGroup as ListGroup
import Bootstrap.Modal as Modal
import Bootstrap.Text as Text
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (getAuthedQuery, historyUrl)
import Html exposing (Html, div, i, text)
import Html.Attributes exposing (class, style)
import Html.Events exposing (onClick)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import JsonViewer exposing (initJsonTree, initJsonTreeCollapsed, updateJsonTree, viewJsonTree)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, resizeAccordions, subscriptionBase, updateBase)
import Main2 exposing (viewBase)
import Utils exposing (buildQuery, fillParent, flex, humanizePascalCase, largeCentered, mediumFont, mediumMargin, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Valid ViewValue
    | Invalid


type alias ViewValue =
    { aggregate : AggregateDescription
    , orderType : OrderType
    , useAdvanced : Bool
    , asOfDate : String
    , asOfTime : String
    , history : WebData History
    }


type OrderType
    = Historical
    | ReverseHistorical


type alias AggregateDescription =
    { name : String
    , id : String
    }


type alias History =
    { snapshots : List ( AggregateSnapshot, Accordion.State )
    , aggregateView : Accordion.State
    , metadataModal : MetadataView
    }


type alias MetadataView =
    { metadata : Maybe JsonViewer.Model
    , view : Modal.Visibility
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
    | MetadataJson JsonViewer.Msg
    | CloseMetadataModal
    | ShowMetadataModal JsonViewer.Model
    | AnimateMetadataModal Modal.Visibility
    | ChangeOrderType Bool
    | ChangeToAdvanced Bool
    | SelectedDate String
    | SelectedTime String


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
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, resizeAccordions () )

                                _ ->
                                    noOp

                        AccordionAggregateMsg state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateHistory =
                                            { history | aggregateView = state }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateHistory }, resizeAccordions () )

                                _ ->
                                    noOp

                        GotAggregate (Err err) ->
                            ( authed <| Valid <| { viewModel | history = Error err }, Cmd.none )

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

                        CloseMetadataModal ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateModal modal =
                                            { modal | view = Modal.hidden, metadata = Nothing }

                                        updateMeta =
                                            { history | metadataModal = updateModal history.metadataModal }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateMeta }, Cmd.none )

                                _ ->
                                    noOp

                        ShowMetadataModal state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateModal modal =
                                            { modal | view = Modal.shown, metadata = Just state }

                                        updateMeta =
                                            { history | metadataModal = updateModal history.metadataModal }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateMeta }, Cmd.none )

                                _ ->
                                    noOp

                        AnimateMetadataModal state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateModal modal =
                                            { modal | view = state }

                                        updateMeta =
                                            { history | metadataModal = updateModal history.metadataModal }
                                    in
                                    ( authed <| Valid <| { viewModel | history = Loaded updateMeta }, Cmd.none )

                                _ ->
                                    noOp

                        MetadataJson state ->
                            case viewModel.history of
                                Loaded history ->
                                    let
                                        updateModal modal meta =
                                            { modal | metadata = Just <| updateJsonTree state meta }

                                        updateMeta meta =
                                            { history | metadataModal = updateModal history.metadataModal meta }
                                    in
                                    case history.metadataModal.metadata of
                                        Just meta ->
                                            ( authed <| Valid <| { viewModel | history = Loaded <| updateMeta meta }, Cmd.none )

                                        Nothing ->
                                            noOp

                                _ ->
                                    noOp

                        ChangeOrderType checked ->
                            let
                                oType =
                                    if checked then
                                        ReverseHistorical

                                    else
                                        Historical
                            in
                            ( authed <| Valid <| { viewModel | orderType = oType, history = Loading }, loadHistoryCmd auth.token oType viewModel.asOfDate viewModel.asOfTime viewModel.aggregate )

                        ChangeToAdvanced checked ->
                            ( authed <| Valid <| { viewModel | useAdvanced = checked }, Cmd.none )

                        SelectedDate date ->
                            ( authed <| Valid <| { viewModel | asOfDate = date, history = Loading }, loadHistoryCmd auth.token viewModel.orderType date viewModel.asOfTime viewModel.aggregate )

                        SelectedTime time ->
                            ( authed <| Valid <| { viewModel | asOfTime = time, history = Loading }, loadHistoryCmd auth.token viewModel.orderType viewModel.asOfDate time viewModel.aggregate )

        _ ->
            ( m, Cmd.none )


updateJsonCommand : CommandData -> JsonViewer.Msg -> CommandData
updateJsonCommand command json =
    { command | payload = updateJsonTree json command.payload }


updateJsonAggregate : AggregateData -> JsonViewer.Msg -> AggregateData
updateJsonAggregate aggregate json =
    { aggregate | payload = updateJsonTree json aggregate.payload }



--commands


loadHistoryCmd : String -> OrderType -> String -> String -> AggregateDescription -> Cmd Msg
loadHistoryCmd token order date time aggregate =
    let
        expect =
            Http.expectJson GotAggregate historyDecoder

        orderValue =
            case order of
                Historical ->
                    0

                ReverseHistorical ->
                    1

        dateTime =
            case date of
                "" ->
                    ""

                _ ->
                    case time of
                        "" ->
                            date

                        _ ->
                            date ++ "T" ++ time

        timeTuple =
            case dateTime of
                "" ->
                    []

                _ ->
                    [ ( "time", dateTime ) ]

        query =
            buildQuery
                ([ ( "name", aggregate.name )
                 , ( "id", aggregate.id )
                 , ( "order", String.fromInt orderValue )
                 ]
                    ++ timeTuple
                )
    in
    getAuthedQuery query token Endpoints.History expect Nothing |> mapCmd


historyDecoder : D.Decoder History
historyDecoder =
    D.field "snapshots" (D.list snapshotDecoder)
        |> D.andThen (\d -> D.succeed <| snapshotMapper d)


snapshotMapper : List AggregateSnapshot -> History
snapshotMapper snapshots =
    History (List.map (\v -> ( v, Accordion.initialState )) snapshots) Accordion.initialState <| MetadataView Nothing Modal.hidden


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
    viewBase model Nothing viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        Valid agg ->
            div [ flex, Flex.col, mediumMargin ]
                [ div [ flex, Flex.row, Flex.alignItemsCenter ] (viewToolbar agg)
                , div [ flex, Flex.row ] [ viewWebdata agg.history <| viewHistory agg.useAdvanced ]
                ]

        Invalid ->
            div [] [ text "Failed to load aggregate" ]


viewToolbar agg =
    let
        isReverse =
            case agg.orderType of
                Historical ->
                    False

                ReverseHistorical ->
                    True
    in
    [ div [ Flex.col, mediumMargin ]
        [ Button.linkButton [ Button.outlineInfo, Button.onClick GoBack, Button.attrs largeCentered ] [ text "Go back" ]
        ]
    , div [ Flex.col, smallMargin ]
        [ Input.date
            [ Input.onInput (\val -> Main <| SelectedDate val)
            , Input.value agg.asOfDate
            ]
        ]
    , div [ Flex.col, smallMargin, style "margin-left" "0.25rem" ] [ i [ class "fa-solid fa-xmark", mediumFont, onClick (Main <| SelectedDate "") ] [] ]
    , div [ Flex.col ]
        [ Input.time
            [ Input.onInput (\val -> Main <| SelectedTime val)
            , Input.value agg.asOfTime
            ]
        ]
    , div [ Flex.col, smallMargin, style "margin-left" "0.25rem" ] [ i [ class "fa-solid fa-xmark", mediumFont, onClick (Main <| SelectedTime "") ] [] ]
    , div (largeCentered ++ [ Flex.col ])
        [ Checkbox.checkbox [ Checkbox.onCheck (\val -> Main <| ChangeOrderType val), Checkbox.checked isReverse ] "Reverse order"
        ]
    , div (largeCentered ++ [ Flex.col, mediumMargin ])
        [ Checkbox.checkbox [ Checkbox.onCheck (\val -> Main <| ChangeToAdvanced val), Checkbox.checked agg.useAdvanced ] "Advanced"
        ]
    ]


viewHistory : Bool -> History -> Html Msg
viewHistory advanced history =
    div fillParent
        [ Accordion.config (\v -> Main <| AccordionAggregateMsg v)
            --|> Accordion.withAnimation
            |> Accordion.cards
                (List.map (\( k, v ) -> viewSnapshot k v advanced) history.snapshots)
            |> Accordion.view history.aggregateView
        , case history.metadataModal.metadata of
            Just meta ->
                Modal.config CloseMetadataModal
                    -- Configure the modal to use animations providing the new AnimateModal msg
                    |> Modal.withAnimation AnimateMetadataModal
                    |> Modal.large
                    |> Modal.h3 [] [ text "Metadata" ]
                    |> Modal.body [] [ JsonViewer.viewJsonTree meta |> Html.map MetadataJson ]
                    |> Modal.footer []
                        [ Button.button
                            [ Button.outlinePrimary
                            , Button.attrs [ onClick <| AnimateMetadataModal Modal.hiddenAnimated ]
                            ]
                            [ text "Close" ]
                        ]
                    |> Modal.view history.metadataModal.view
                    |> Html.map Main

            Nothing ->
                div [] []
        ]


viewSnapshot : AggregateSnapshot -> Accordion.State -> Bool -> Accordion.Card Msg
viewSnapshot snapshot state advanced =
    let
        id =
            snapshot.lastCommand.metadata.id

        related =
            if List.length snapshot.related == 0 then
                []

            else
                [ div largeCentered [ text "Related" ]
                , ListGroup.ul
                    (List.map
                        (\rel ->
                            ListGroup.li [ ListGroup.dark ]
                                [ Button.linkButton
                                    [ Button.outlinePrimary
                                    , Button.onClick <| Navigate <| historyUrl rel.name rel.id
                                    ]
                                    [ text rel.role ]
                                ]
                        )
                        snapshot.related
                    )
                ]
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
                                ([ div largeCentered [ text "State" ]
                                 , viewJsonTree snapshot.aggregate.payload |> Html.map (\v -> Main <| AggregateDataJson snapshot v)
                                 , viewMetaBtn snapshot.aggregate.metadata advanced
                                    |> Html.map Main
                                 ]
                                    ++ related
                                )
                            , div [ flex, Flex.col, style "width" "50%" ]
                                [ div largeCentered [ text "Command" ]
                                , Accordion.config (AccordionSnapshotMsg snapshot)
                                    -- |> Accordion.withAnimation
                                    |> Accordion.cards
                                        ([ viewCommandData snapshot snapshot.lastCommand advanced ]
                                            ++ List.map (viewSnapshotEvent snapshot advanced) snapshot.events
                                        )
                                    |> Accordion.view state
                                    |> Html.map Main
                                ]
                            ]
                        ]
                ]
            ]
        }


viewMetaBtn : D.Value -> Bool -> Html LocalMsg
viewMetaBtn meta advanced =
    if advanced then
        Button.button
            [ Button.secondary
            , Button.onClick <| ShowMetadataModal <| JsonViewer.initJsonTree meta
            ]
            [ text "Metadata" ]

    else
        div [] []


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
                " requested "
    in
    div (largeCentered ++ [ class textColor ]) [ text ("\"" ++ snapshot.lastCommand.metadata.userName ++ "\"" ++ actionName ++ "\"" ++ humanizePascalCase snapshot.lastCommand.metadata.name ++ "\"" ++ " " ++ snapshot.displayTime) ]


viewCommandData : AggregateSnapshot -> CommandData -> Bool -> Accordion.Card LocalMsg
viewCommandData snapshot command advanced =
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
                [ Block.custom <|
                    div []
                        [ viewJsonTree command.payload |> Html.map (CommandDataJson snapshot)
                        , viewMetaBtn command.metadata.fullMetadata advanced
                        ]
                ]
            ]
        }


viewSnapshotEvent : AggregateSnapshot -> Bool -> EventData -> Accordion.Card LocalMsg
viewSnapshotEvent snapshot advanced event =
    Accordion.card
        { id = event.metadata.id
        , options = [ Card.outlineSecondary, Card.align Text.alignXsCenter ]
        , header =
            Accordion.header [] <| Accordion.toggle [] [ text <| ("Event \"" ++ humanizePascalCase event.metadata.name ++ "\"") ]
        , blocks =
            [ Accordion.block []
                [ Block.custom <|
                    div []
                        [ viewJsonTree event.payload |> Html.map (EventDataJson snapshot event)
                        , viewMetaBtn event.metadata.fullMetadata advanced
                        ]
                ]
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
                    Valid { aggregate = agg, history = Loading, orderType = Historical, useAdvanced = False, asOfDate = "", asOfTime = "" }

                Err err ->
                    Invalid

        initialCmd res =
            case initialState of
                Valid agg ->
                    loadHistoryCmd res.token agg.orderType agg.asOfDate agg.asOfTime agg.aggregate

                Invalid ->
                    Cmd.none
    in
    initBase [ Manager ] initialState initialCmd resp


subscriptions : Model -> Sub Msg
subscriptions model =
    subscriptionBase model
        (case model of
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
                                    ++ [ Accordion.subscriptions history.aggregateView (\st -> Main <| AccordionAggregateMsg st)
                                       , Modal.subscriptions history.metadataModal.view (\vis -> Main <| AnimateMetadataModal vis)
                                       ]
                                    |> Sub.batch

                            _ ->
                                Sub.none

                    _ ->
                        Sub.none

            _ ->
                Sub.none
        )


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
