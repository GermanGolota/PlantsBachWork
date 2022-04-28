module Pages.Stats exposing (main)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input exposing (date)
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.Table as Table
import Dict exposing (Dict)
import Endpoints exposing (Endpoint(..), getAuthed)
import Html exposing (Html, div, h1, i, text)
import Html.Attributes exposing (class, style)
import Http
import Iso8601 exposing (toTime)
import Json.Decode as D
import Json.Decode.Pipeline exposing (hardcoded, required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (navView, statsLink)
import PieChart exposing (Msg(..), pieChartWithLabel)
import Time
import Utils exposing (AlignDirection(..), fillParent, flatten, itself, largeFont, textAlign, textCenter, unique, viewLoading)
import Webdata exposing (WebData(..), viewWebdata)



--update


type Msg
    = PieEvent PieChart.Msg
    | Switched
    | DateLeftSelected String
    | DateRightSelected String
    | GotFinancial (Result Http.Error (List FinancialPieItem))
    | GotTotals (Result Http.Error (List TotalsPieItem))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    let
        switchView viewType token =
            case viewType of
                Totals _ ->
                    ( Financials NoDateSelected, Cmd.none )

                Financials _ ->
                    ( Totals Loading, getTotals token )

        convertToTime dateStr =
            Time.posixToMillis <| Result.withDefault (Time.millisToPosix 0) (toTime dateStr)

        datesValid from to =
            convertToTime from <= convertToTime to

        updateOnDate from to token =
            if datesValid from to then
                ( Authorized token <| Financials (BothSelected from to (ValidDates Loading)), getFin from to token.token )

            else
                ( Authorized token <| Financials (BothSelected from to BadDates), Cmd.none )
    in
    case ( msg, model ) of
        ( PieEvent pieEvent, Authorized token (Totals (Loaded viewType)) ) ->
            case pieEvent of
                ChartItemClicked plantId ->
                    let
                        getById id =
                            List.head <| List.filter (\item -> item.id == id) viewType.items

                        totals =
                            Authorized token (Totals <| Loaded { viewType | selectedItem = getById plantId })
                    in
                    ( totals, Cmd.none )

        ( PieEvent pieEvent, Authorized token (Financials (BothSelected from to (ValidDates (Loaded fin)))) ) ->
            case pieEvent of
                ChartItemClicked plantId ->
                    let
                        getById id =
                            List.head <| List.filter (\item -> item.id == id) fin.items

                        finA =
                            Authorized token (Financials <| BothSelected from to <| ValidDates <| Loaded { fin | selectedItem = getById plantId })
                    in
                    ( finA, Cmd.none )

        ( Switched, Authorized token viewType ) ->
            Tuple.mapFirst (\viewType2 -> Authorized token viewType2) (switchView viewType token.token)

        ( GotTotals (Ok res), Authorized token (Totals _) ) ->
            ( Authorized token (Totals <| Loaded <| TotalsView res Nothing), Cmd.none )

        ( GotTotals (Err err), Authorized token (Totals _) ) ->
            ( Authorized token (Totals <| Error), Cmd.none )

        ( DateLeftSelected date, Authorized token (Financials NoDateSelected) ) ->
            ( Authorized token <| Financials <| OnlyLeftSelected date, Cmd.none )

        ( DateRightSelected date, Authorized token (Financials NoDateSelected) ) ->
            ( Authorized token <| Financials <| OnlyRightSelected date, Cmd.none )

        ( DateLeftSelected left, Authorized token (Financials (OnlyRightSelected right)) ) ->
            updateOnDate left right token

        ( DateLeftSelected left, Authorized token (Financials (BothSelected oldLeft right _)) ) ->
            updateOnDate left right token

        ( DateRightSelected right, Authorized token (Financials (OnlyLeftSelected left)) ) ->
            updateOnDate left right token

        ( DateRightSelected right, Authorized token (Financials (BothSelected left oldRight _)) ) ->
            updateOnDate left right token

        ( GotFinancial (Ok res), Authorized token (Financials (BothSelected left right (ValidDates _))) ) ->
            ( Authorized token <| Financials <| BothSelected left right <| ValidDates <| Loaded <| FinancialView res Nothing, Cmd.none )

        ( GotFinancial (Err err), Authorized token (Financials (BothSelected left right (ValidDates _))) ) ->
            ( Authorized token <| Financials <| BothSelected left right <| ValidDates <| Error, Cmd.none )

        ( _, _ ) ->
            ( model, Cmd.none )



--view


dateInput : (String -> msg) -> Html msg
dateInput onInput =
    Input.date [ Input.onInput onInput ]


viewPies : List PieSlice -> Html PieChart.Msg
viewPies items =
    let
        sorted =
            sortPies items

        keys =
            Dict.keys sorted

        getPies key =
            ( key, Maybe.withDefault [] <| Dict.get key sorted )

        toCol ( key, pies ) =
            Grid.col [] <|
                pieChartWithLabel
                    key
                    (List.map .id pies)
                    (List.map .value pies)
                    (List.map .text pies)
    in
    Grid.row [] <|
        List.map toCol (List.map getPies keys)


sortPies : List PieSlice -> Dict String (List PieSlice)
sortPies pies =
    let
        names =
            List.map .name pies |> unique

        createTuple name =
            ( name, [] )

        emptyDict =
            Dict.fromList <| List.map createTuple names

        slicesByName name =
            List.filter (\pie -> pie.name == name) pies

        updateDict name =
            Dict.update name (Maybe.map (\arr -> arr ++ slicesByName name))
    in
    List.foldl updateDict emptyDict names


viewSelected : String -> ( String, Float ) -> ( String, Float ) -> ( String, Float ) -> Html msg
viewSelected textMain ( h1, v1 ) ( h2, v2 ) ( h3, v3 ) =
    let
        rowConvert header value =
            viewRow header <| String.fromFloat value
    in
    Table.table
        { options = [ Table.striped, Table.hover ]
        , thead =
            Table.simpleThead
                [ Table.th
                    [ Table.cellAttr textCenter
                    , Table.cellAttr <| Html.Attributes.attribute "colspan" "2"
                    ]
                    [ text textMain ]
                ]
        , tbody =
            Table.tbody []
                [ rowConvert h1 v1
                , rowConvert h2 v2
                , rowConvert h3 v3
                ]
        }


viewRow : String -> String -> Table.Row msg
viewRow key value =
    Table.tr []
        [ Table.td [ Table.cellAttr <| textAlign Left ] [ text key ]
        , Table.td [ Table.cellAttr <| textAlign Right ] [ text <| value ]
        ]


view : Model -> Html.Html Msg
view model =
    viewBase viewMain model


viewMain : AuthResponse -> View -> Html Msg
viewMain resp model =
    let
        localizedView =
            case model of
                Totals totals ->
                    viewWebdata totals viewTotalsMain

                Financials financials ->
                    viewFinancials financials

        localizedTitle =
            case model of
                Totals _ ->
                    "Totals"

                Financials _ ->
                    "Financials"
    in
    navView
        "Username"
        resp.roles
        (Just
            statsLink
        )
        (div fillParent
            [ getSwitchButtonFor model
            , h1 [ textCenter ] [ text localizedTitle ]
            , localizedView
            ]
        )


viewFinancials : FinancialViewType -> Html Msg
viewFinancials fin =
    let
        bodyView =
            case fin of
                BothSelected from to viewType ->
                    case viewType of
                        BadDates ->
                            div [] [ text "Dates do not match" ]

                        ValidDates data ->
                            viewWebdata data viewFinancialsMain

                _ ->
                    div [] []
    in
    Grid.container
        []
        [ div [] [ datesRow, bodyView ] ]


viewFinancialsMain : FinancialView -> Html Msg
viewFinancialsMain fin =
    let
        items =
            fin.items

        selectedTable item =
            viewSelected item.text ( "Income", item.income ) ( "Sold %", item.percentSold ) ( "Sold Count", item.soldCount )
    in
    div []
        [ Html.map
            convertToEvent
            (viewPies <| getSlicesForFinancial items)
        , viewSelectedBase fin.selectedItem selectedTable
        ]


getSwitchButtonFor : View -> Html Msg
getSwitchButtonFor viewType =
    let
        buttonText =
            case viewType of
                Totals _ ->
                    "Go to financial"

                Financials _ ->
                    "Go to totals"

        rightIcon =
            case viewType of
                Totals _ ->
                    i [ class "fa-solid fa-arrow-right", style "margin-left" "1em" ] []

                _ ->
                    div [] []

        leftIcon =
            case viewType of
                Financials _ ->
                    i [ class "fa-solid fa-arrow-left", style "margin-right" "1em" ] []

                _ ->
                    div [] []
    in
    Button.button [ Button.primary, Button.attrs [ style "float" "right" ], Button.onClick Switched ]
        [ leftIcon
        , text buttonText
        , rightIcon
        ]


viewTotalsMain : TotalsView -> Html Msg
viewTotalsMain model =
    let
        items =
            model.items

        selectedTable item =
            viewSelected item.text ( "Income", item.income ) ( "Instructions", item.instructions ) ( "Popularity", item.popularity )
    in
    Html.map
        convertToEvent
        (Grid.container
            []
            [ viewPies <| getSlicesForTotals items
            , viewSelectedBase model.selectedItem selectedTable
            ]
        )


viewSelectedBase : Maybe a -> (a -> Html msg) -> Html msg
viewSelectedBase item selectTable =
    Grid.row
        [ Row.attrs [ Html.Attributes.style "margin-top" "0.5em", textCenter, largeFont ]
        ]
        [ Grid.col []
            [ case item of
                Just value ->
                    selectTable value

                Nothing ->
                    Html.text "No group is selected"
            ]
        ]


datesRow : Html Msg
datesRow =
    let
        leftSelected str =
            DateLeftSelected str

        rightSelected str =
            DateRightSelected str
    in
    Grid.row
        [ Row.centerXs, Row.attrs [ textCenter, Html.Attributes.style "align-items" "center" ] ]
        [ Grid.col []
            [ dateInput leftSelected
            ]
        , Grid.col
            [ Col.xsAuto
            , Col.attrs
                [ largeFont
                ]
            ]
            [ text "-" ]
        , Grid.col []
            [ dateInput rightSelected
            ]
        ]


convertToEvent : PieChart.Msg -> Msg
convertToEvent msg =
    PieEvent msg


getSlicesForTotals : List TotalsPieItem -> List PieSlice
getSlicesForTotals pies =
    flatten <| List.map getSlices pies


getSlicesForFinancial : List FinancialPieItem -> List PieSlice
getSlicesForFinancial pies =
    flatten <| List.map getSlicesF pies


getSlices : TotalsPieItem -> List PieSlice
getSlices item =
    let
        base val text =
            PieSlice item.id item.text val text
    in
    [ base item.income "Income", base item.instructions "Instructions", base item.popularity "Popularity" ]


getSlicesF : FinancialPieItem -> List PieSlice
getSlicesF item =
    let
        base val text =
            PieSlice item.id item.text val text
    in
    [ base item.income "Income", base item.percentSold "Sold %", base item.soldCount "Sold Count" ]



--Model


type View
    = Totals (WebData TotalsView)
    | Financials FinancialViewType


type alias TotalsView =
    { items : List TotalsPieItem
    , selectedItem : Maybe TotalsPieItem
    }


type alias TotalsPieItem =
    { id : Int, text : String, income : Float, instructions : Float, popularity : Float }


type alias PieSlice =
    { id : Int
    , text : String
    , value : Float
    , name : String
    }


type FinancialViewType
    = NoDateSelected
    | OnlyLeftSelected String
    | OnlyRightSelected String
    | BothSelected String String BothSelectedViewType


type BothSelectedViewType
    = ValidDates (WebData FinancialView)
    | BadDates


type alias FinancialView =
    { items : List FinancialPieItem
    , selectedItem : Maybe FinancialPieItem
    }


type alias FinancialPieItem =
    { id : Int, text : String, soldCount : Float, percentSold : Float, income : Float }


type alias Model =
    ModelBase View



--decoders


totalsDecoder : D.Decoder (List TotalsPieItem)
totalsDecoder =
    D.field "groups" <| D.list totalItemDecoder


financialDecoder : D.Decoder (List FinancialPieItem)
financialDecoder =
    D.field "groups" <| D.list financialItemDecoder


totalItemDecoder : D.Decoder TotalsPieItem
totalItemDecoder =
    D.succeed TotalsPieItem
        |> required "groupId" D.int
        |> required "groupName" D.string
        |> required "income" D.float
        |> required "instructions" D.float
        |> required "popularity" D.float


financialItemDecoder : D.Decoder FinancialPieItem
financialItemDecoder =
    D.succeed FinancialPieItem
        |> required "groupId" D.int
        |> required "groupName" D.string
        |> required "soldCount" D.float
        |> required "percentSold" D.float
        |> required "income" D.float



--init


init : Maybe AuthResponse -> ( Model, Cmd Msg )
init response =
    let
        token =
            case response of
                Just item ->
                    item.token

                Nothing ->
                    ""

        roles =
            case response of
                Just item ->
                    item.roles

                Nothing ->
                    []
    in
    initBase [ Manager ] ( Totals Loading, getTotals token ) response


getTotals : String -> Cmd Msg
getTotals token =
    Endpoints.getAuthed token StatsTotal (Http.expectJson GotTotals totalsDecoder) Nothing



--getAuthed


getFin : String -> String -> String -> Cmd Msg
getFin from to token =
    Endpoints.getAuthedQuery ("?from=" ++ from ++ "&to=" ++ to) token StatsFinancial (Http.expectJson GotFinancial financialDecoder) Nothing


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
