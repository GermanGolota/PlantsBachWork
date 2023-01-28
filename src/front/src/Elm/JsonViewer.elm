module JsonViewer exposing (..)

import Html exposing (Html, button, div, h3, input, label, pre, text)
import Html.Attributes exposing (checked, type_)
import Html.Events exposing (onCheck, onClick)
import Json.Decode as D
import JsonTree exposing (defaultColors)


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
    div []
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
