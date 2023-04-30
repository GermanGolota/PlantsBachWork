module PieChart exposing (Msg(..), pieChart, pieChartWithLabel)

import Array exposing (Array)
import Color exposing (Color)
import Html exposing (Html)
import Html.Attributes
import Murmur3 exposing (hashString)
import Path
import Shape exposing (defaultPieConfig)
import Svg
import TypedSvg exposing (g, svg, text_)
import TypedSvg.Attributes exposing (dy, fill, fontFamily, fontSize, fontWeight, stroke, textAnchor, transform, viewBox)
import TypedSvg.Core exposing (text)
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
        , Color.rgb255 229 225 204
        , Color.rgb255 255 99 71
        , Color.rgb255 218 165 32
        , Color.rgb255 138 43 226
        , Color.rgb255 245 222 179
        , Color.rgb255 210 105 30
        , Color.rgb255 255 228 225
        , Color.rgb255 32 178 170
        ]


radius : Float
radius =
    min w h / 2


pieSlice : String -> Shape.Arc -> Svg.Svg Msg
pieSlice index datum =
    Path.element (Shape.arc datum) [ createFill (hashString 1234 index), stroke (Paint Color.white), onClick <| ChartItemClicked index ]


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
        filtered =
            List.filter (\v -> not (v == 0)) values

        pieData =
            filtered |> Shape.pie { defaultPieConfig | outerRadius = radius }

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
