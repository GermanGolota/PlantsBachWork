module Pages.Stats exposing (main)

import Bootstrap.CDN as CDN
import Bootstrap.Form.Input as Input
import Bootstrap.Grid as Grid
import Bootstrap.Grid.Col as Col
import Bootstrap.Grid.Row as Row
import Bootstrap.Table as Table
import Html exposing (Html, text)
import Html.Attributes
import Json.Decode as D
import Main exposing (baseApplication)
import PieChart exposing (Msg(..), pieChartWithLabel)
import Utils exposing (AlignDirection(..), largeFont, textAlign, textCenter)


type Msg
    = PieEvent PieChart.Msg


dateInput : Html msg
dateInput =
    Input.date []


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        PieEvent pieEvent ->
            case pieEvent of
                ChartItemClicked plantId ->
                    let
                        getById id =
                            List.head <| List.filter (\item -> item.id == id) model.items
                    in
                    ( { model | selectedItem = getById plantId }, Cmd.none )


type alias PieItem =
    { id : Int, text : String, income : Float, instructions : Float, popularity : Float }


viewPies : List PieItem -> Html PieChart.Msg
viewPies items =
    let
        ids =
            items |> List.map .id

        labels =
            items |> List.map .text

        incomes =
            items |> List.map .income

        instructions =
            items |> List.map .instructions

        popularitys =
            items |> List.map .popularity
    in
    Grid.row []
        [ Grid.col []
            (pieChartWithLabel
                "Income"
                ids
                incomes
                labels
            )
        , Grid.col []
            (pieChartWithLabel
                "Instructions"
                ids
                instructions
                labels
            )
        , Grid.col []
            (pieChartWithLabel
                "Popularity"
                ids
                popularitys
                labels
            )
        ]


viewSelected : PieItem -> Html msg
viewSelected item =
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
                    [ text item.text ]
                ]
        , tbody =
            Table.tbody []
                [ rowConvert "Income" item.income
                , rowConvert "Popularity" item.popularity
                , rowConvert "Instructions" item.instructions
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
    let
        items =
            model.items
    in
    Html.map
        convertToEvent
        (Grid.container
            []
            [ CDN.stylesheet
            , Grid.row [ Row.centerXs, Row.attrs [ textCenter, Html.Attributes.style "align-items" "center" ] ] [ Grid.col [] [ dateInput ], Grid.col [ Col.xsAuto, Col.attrs [ largeFont ] ] [ text "-" ], Grid.col [] [ dateInput ] ]
            , viewPies items
            , Grid.row [ Row.attrs [ Html.Attributes.style "margin-top" "0.5em", textCenter, largeFont ] ]
                [ Grid.col []
                    [ case model.selectedItem of
                        Just item ->
                            viewSelected item

                        Nothing ->
                            Html.text "No group is selected"
                    ]
                ]
            ]
        )


convertToEvent : PieChart.Msg -> Msg
convertToEvent msg =
    PieEvent msg


type alias Model =
    { items : List PieItem
    , selectedItem : Maybe PieItem
    }


init : String -> ( Model, Cmd Msg )
init authToken =
    ( initialModel, Cmd.none )


initialModel : Model
initialModel =
    Model
        [ PieItem 1 "Apple" 100 25 5
        , PieItem 2 "Plum" 32 14 6
        , PieItem 3 "Cherry" 15 15 3
        , PieItem 4 "Cactus" 20 20 1
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
