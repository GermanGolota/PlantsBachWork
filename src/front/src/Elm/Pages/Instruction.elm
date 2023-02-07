module Pages.Instruction exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), historyUrl)
import Html exposing (Html, div, img, text)
import Html.Attributes exposing (alt, src, style)
import Http
import InstructionHelper exposing (InstructionView, getInstruction)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, isAdmin, mapCmd, subscriptionBase, updateBase)
import Main2 exposing (viewBase2)
import NavBar exposing (instructionsLink)
import Utils exposing (decodeId, fillParent, flex, flex1, largeCentered, mediumMargin, smallMargin, textCenter, textHtml)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Instruction (WebData InstructionView)
    | NoInstruction



--update


type LocalMsg
    = NoOp
    | GotInstruction (Result Http.Error (Maybe InstructionView))


type alias Msg =
    MsgBase LocalMsg


update : Msg -> Model -> ( Model, Cmd Msg )
update =
    updateBase updateLocal


updateLocal : LocalMsg -> Model -> ( Model, Cmd Msg )
updateLocal msg m =
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
                    ( authed newModel, Cmd.none )
            in
            case msg of
                GotInstruction (Ok (Just res)) ->
                    updateModel <| Instruction <| Loaded res

                GotInstruction (Ok Nothing) ->
                    updateModel <| NoInstruction

                GotInstruction (Err err) ->
                    updateModel <| Instruction <| Error err

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--view


view : Model -> Html Msg
view model =
    viewBase2 model (Just instructionsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        NoInstruction ->
            div largeCentered [ text "There is no such instruction" ]

        Instruction ins ->
            let
                historyBtn =
                    if isAdmin resp then
                        case ins of
                            Loaded i ->
                                Button.linkButton
                                    [ Button.outlinePrimary
                                    , Button.onClick <| Navigate <| historyUrl "PlantInstruction" i.id
                                    , Button.attrs [ smallMargin ]
                                    ]
                                    [ text "View history" ]

                            _ ->
                                div [] []

                    else
                        div [] []
            in
            viewWebdata ins (viewInstruction historyBtn)


viewInstruction : Html Msg -> InstructionView -> Html Msg
viewInstruction historyBtn ins =
    let
        viewDesc txt =
            div largeCentered [ text txt ]

        viewText txt =
            div [ textCenter ] [ text txt ]
    in
    div ([ flex, Flex.col ] ++ fillParent)
        [ div [ flex, Flex.row, flex1 ]
            [ div [ flex1 ]
                [ img ([ src <| Maybe.withDefault "" ins.imageUrl, alt "No cover", style "max-width" "50%" ] ++ fillParent)
                    []
                ]
            , div [ flex, Flex.col, flex1 ]
                [ viewDesc "Title"
                , viewText ins.title
                , viewDesc "Description"
                , viewText ins.description
                ]
            ]
        , div ([ flex, style "flex" "0.5", Flex.row, Flex.justifyCenter ] ++ largeCentered) [ text "Instruction Content" ]
        , div [ flex, Flex.col, mediumMargin, style "flex" "8", style "overflow-y" "scroll" ]
            [ div fillParent
                [ Html.p [] (textHtml ins.text) ]
            ]
        , div [ style "flex" "0.5", flex, Flex.row, Flex.justifyCenter, smallMargin ]
            [ Button.linkButton [ Button.outlinePrimary, Button.onClick <| Navigate "/instructions" ] [ text "Go back" ]
            , historyBtn
            ]
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        insId =
            D.decodeValue (D.field "id" decodeId) flags

        initialModel =
            case insId of
                Ok id ->
                    Instruction Loading

                Err _ ->
                    NoInstruction

        initialCmd res =
            case insId of
                Ok id ->
                    getInstruction GotInstruction res.token id |> mapCmd

                Err _ ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initialModel initialCmd resp



--subs


subscriptions : Model -> Sub Msg
subscriptions model =
    subscriptionBase model Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
