module ImageList exposing (..)

import Bootstrap.Utilities.Flex as Flex
import Dict exposing (Dict)
import Html exposing (Html, div, figure, i, text)
import Html.Attributes exposing (alt, class, src, style)
import Html.Events exposing (onClick)
import Utils exposing (fillParent, flex, flex1, mediumMargin, smallMargin)


type Msg
    = ImageSwitched String
    | Clicked String


type alias Model =
    { selected : Maybe String, available : Dict String String }


fromDict : Dict String String -> Model
fromDict dict =
    let
        selected =
            List.head <| List.map Tuple.first (Dict.toList dict)
    in
    Model selected dict


update : Msg -> Model -> Model
update msg model =
    case msg of
        ImageSwitched num ->
            Model (Just num) model.available

        Clicked _ ->
            model


view : Model -> Html Msg
view model =
    let
        available =
            Dict.toList model.available
    in
    case available of
        [] ->
            div [] [ text "No available images" ]

        _ ->
            let
                selected =
                    case model.selected of
                        Just index ->
                            index

                        Nothing ->
                            Tuple.first <| Maybe.withDefault ( "-1", "" ) <| List.head available
            in
            case Dict.get selected model.available of
                Just url ->
                    let
                        keys =
                            List.map Tuple.first available

                        keyOfSelected =
                            Maybe.withDefault "-1" <| List.head <| List.map (\item -> Tuple.first item) <| List.filter (\item -> Tuple.second item == url) available

                        isSelected key =
                            key == keyOfSelected
                    in
                    div (fillParent ++ [ mediumMargin, flex, Flex.col ])
                        [ Html.img ([ src url, alt "No images for this plant", style "max-width" "70%", style "max-height" "70%", flex1, onClick (Clicked <| Maybe.withDefault "-1" model.selected) ] ++ imageCenter) []
                        , div [ flex, Flex.row, Flex.justifyCenter, mediumMargin ]
                            (List.map
                                (\key -> viewIcon (isSelected key) key)
                                keys
                            )
                        ]

                Nothing ->
                    div [] [ text "Something went wrong" ]



--<i class="fa-solid fa-circle"></i>
--<i class="fa-solid fa-circle-notch"></i>


viewIcon isSelected index =
    let
        iClass =
            if isSelected then
                "fa-solid fa-circle"

            else
                "fa-solid fa-circle-notch"
    in
    i [ class iClass, onClick <| ImageSwitched index, smallMargin ] []


imageCenter =
    [ style "display" "block"
    , style "margin-left" "auto"
    , style "margin-right" "auto"
    , style "width" "100%"
    ]
