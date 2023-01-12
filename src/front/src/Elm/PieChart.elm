module PieChart exposing (Msg(..), pieChart, pieChartWithLabel)

import Array exposing (Array)
import Color exposing (Color)
import Html exposing (Html)
import Html.Attributes exposing (style)
import Path
import Shape exposing (defaultPieConfig)
import Svg exposing (Svg)
import TypedSvg exposing (g, svg, text_)
import TypedSvg.Attributes exposing (dy, fill, fontFamily, fontSize, fontWeight, stroke, textAnchor, transform, viewBox)
import TypedSvg.Attributes.InPx exposing (height, width)
import TypedSvg.Core exposing (Svg, text)
import TypedSvg.Events exposing (onClick)
import TypedSvg.Types exposing (AnchorAlignment(..), FontWeight(..), Paint(..), Transform(..), em, rem)


type Msg
    = ChartItemClicked String


w : Float
w =
    500


h : Float
h =
    500


colors : Array Color
colors =
    Array.fromList
        [ Color.rgb255 152 171 198
        , Color.rgb255 138 137 166
        , Color.rgb255 123 104 136
        , Color.rgb255 107 72 107
        , Color.rgb255 159 92 85
        , Color.rgb255 208 116 60
        , Color.rgb255 255 96 0
        ]


radius : Float
radius =
    min w h / 2


pieSlice : String -> Shape.Arc -> Svg.Svg Msg
pieSlice index datum =
    Path.element (Shape.arc datum) [ createFill (String.length index), stroke (Paint Color.white), onClick <| ChartItemClicked index ]


createFill : Int -> Svg.Attribute msg
createFill index =
    fill <| Paint <| Maybe.withDefault Color.black <| Array.get (colorIndex index) colors


colorIndex : Int -> Int
colorIndex index =
    modBy (colors |> Array.length) index


pieLabel : Shape.Arc -> String -> Html Msg
pieLabel slice label =
    let
        ( x, y ) =
            Shape.centroid { slice | innerRadius = radius - 40, outerRadius = radius - 40 }
    in
    text_
        [ transform [ Translate x y ]
        , dy (em 0.35)
        , textAnchor AnchorMiddle
        , fontWeight FontWeightBold
        , fontFamily [ "Helvetica", "sans-serif" ]
        , fontSize (rem 1.125)
        ]
        [ text label ]


pieChart : List String -> List Float -> List String -> Svg.Svg Msg
pieChart ids values labels =
    let
        pieData =
            values |> Shape.pie { defaultPieConfig | outerRadius = radius }

        idToPie =
            List.map2 Tuple.pair ids pieData
    in
    svg
        [ viewBox 0 0 w h ]
        [ g [ transform [ Translate (w / 2) (h / 2) ] ]
            [ g [] <| List.map (\data -> pieSlice (Tuple.first data) (Tuple.second data)) idToPie
            , g [] (List.map2 pieLabel pieData labels)
            ]
        ]


pieChartWithLabel : String -> List String -> List Float -> List String -> List (Html Msg)
pieChartWithLabel label ids values labels =
    [ Html.div
        [ Html.Attributes.style "text-align" "center", Html.Attributes.style "font-size" "3rem" ]
        [ Html.text label ]
    , pieChart ids values labels
    ]
