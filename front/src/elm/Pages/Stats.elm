module Pages.Stats exposing (main)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.Table as Table
import Dict exposing (Dict)
import Html exposing (Html, div, h1, i, text)
import Html.Attributes exposing (class, style)
import Json.Decode as D
import Json.Decode.Pipeline exposing (hardcoded, required)
import Json.Encode as E
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import PieChart exposing (Msg(..), pieChartWithLabel)
import Utils exposing (AlignDirection(..), flatten, largeFont, textAlign, textCenter, unique)



--update


type Msg
    = PieEvent PieChart.Msg
    | Switched
    | DateLeftSelected String
    | DateRightSelected String


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    let
        switchView viewType =
            case viewType of
                Totals _ ->
                    initialFinancial

                Financials _ ->
                    initialTotals
    in
    case ( msg, model ) of
        ( PieEvent pieEvent, Authorized (Totals viewType) ) ->
            case pieEvent of
                ChartItemClicked plantId ->
                    let
                        getById id =
                            List.head <| List.filter (\item -> item.id == id) viewType.items
                    in
                    ( Authorized <| Totals { viewType | selectedItem = getById plantId }, Cmd.none )

        ( PieEvent pieEvent, Authorized (Financials fin) ) ->
            case pieEvent of
                ChartItemClicked plantId ->
                    let
                        getById id =
                            List.head <| List.filter (\item -> item.id == id) fin.items
                    in
                    ( Authorized <| Financials { fin | selectedItem = getById plantId }, Cmd.none )

        ( Switched, Authorized viewType ) ->
            ( Authorized <| switchView viewType, Cmd.none )

        ( DateLeftSelected date, Authorized (Financials fin) ) ->
            ( Authorized <| Financials <| { fin | dateLeft = date }, Cmd.none )

        ( DateRightSelected date, Authorized (Financials fin) ) ->
            ( Authorized <| Financials <| { fin | dateRight = date }, Cmd.none )

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


viewMain : View -> Html Msg
viewMain model =
    let
        localizedView =
            case model of
                Totals totals ->
                    viewTotals totals

                Financials financials ->
                    viewFinancials financials

        localizedTitle =
            case model of
                Totals _ ->
                    "Totals"

                Financials _ ->
                    "Financials"
    in
    Grid.container
        []
        [ getSwitchButtonFor model
        , h1 [ textCenter ] [ text localizedTitle ]
        , localizedView
        ]


viewFinancials : FinancialView -> Html Msg
viewFinancials fin =
    let
        items =
            fin.items

        selectedTable item =
            viewSelected item.text ( "Income", item.income ) ( "Sold %", item.percentSold ) ( "Sold Count", item.soldCount )
    in
    Grid.container
        []
        [ div [] [ datesRow ]
        , Html.map
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


viewTotals : TotalsView -> Html Msg
viewTotals model =
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



--Model


type View
    = Totals TotalsView
    | Financials FinancialView


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
    [ base item.income "Income", base item.percentSold "Sold %", base item.soldCount "Sold number" ]


type alias FinancialView =
    { dateLeft : String
    , dateRight : String
    , items : List FinancialPieItem
    , selectedItem : Maybe FinancialPieItem
    }


type alias FinancialPieItem =
    { id : Int, text : String, soldCount : Float, percentSold : Float, income : Float }


type alias Model =
    ModelBase View



--init


init : Maybe AuthResponse -> ( Model, Cmd Msg )
init response =
    initBase [ Manager ] ( initialModel, Cmd.none ) response


initialModel : View
initialModel =
    initialTotals


initialFinancial : View
initialFinancial =
    Financials <|
        FinancialView ""
            ""
            [ FinancialPieItem 1 "Apple" 100 25 5
            , FinancialPieItem 2 "Plum" 32 14 6
            , FinancialPieItem 3 "Cherry" 15 15 3
            , FinancialPieItem 4 "Cactus" 20 20 1
            ]
            Nothing


initialTotals : View
initialTotals =
    Totals <|
        TotalsView
            [ TotalsPieItem 1 "Apple" 100 25 5
            , TotalsPieItem 2 "Plum" 32 14 6
            , TotalsPieItem 3 "Cherry" 15 15 3
            , TotalsPieItem 4 "Cactus" 20 20 1
            ]
            Nothing


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
