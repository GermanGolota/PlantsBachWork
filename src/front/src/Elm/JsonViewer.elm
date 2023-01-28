module JsonViewer exposing (..)

import Html exposing (Html, button, div, pre, text)
import Html.Events exposing (onClick)
import Json.Decode as D
import JsonTree exposing (defaultColors)
import Utils exposing (AlignDirection(..), textAlign)


type Msg
    = SetTreeViewState JsonTree.State
    | ExpandAll
    | CollapseAll


type alias Model =
    { state : JsonTree.State
    , parseResult : Result D.Error JsonTree.Node
    }


viewJsonTree : Model -> Html Msg
viewJsonTree model =
    let
        toolbar =
            div []
                [ button [ onClick ExpandAll ] [ text "Expand All" ]
                , button [ onClick CollapseAll ] [ text "Collapse All" ]
                ]

        config =
            { colors = defaultColors
            , onSelect = Nothing
            , toMsg = SetTreeViewState
            }
    in
    div [ textAlign Left ]
        [ toolbar
        , case model.parseResult of
            Ok rootNode ->
                JsonTree.view rootNode config model.state

            Err e ->
                pre [] [ text ("Invalid JSON: " ++ D.errorToString e) ]
        ]


initJsonTree : D.Value -> Model
initJsonTree json =
    { state = JsonTree.defaultState
    , parseResult = JsonTree.parseValue json
    }


updateJsonTree : Msg -> Model -> Model
updateJsonTree msg model =
    case msg of
        SetTreeViewState state ->
            { model | state = state }

        ExpandAll ->
            { model | state = JsonTree.expandAll model.state }

        CollapseAll ->
            case model.parseResult of
                Ok rootNode ->
                    { model | state = JsonTree.collapseToDepth 1 rootNode model.state }

                Err _ ->
                    model
