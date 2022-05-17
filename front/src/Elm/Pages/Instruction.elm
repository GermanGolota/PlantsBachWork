module Pages.Instruction exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed, instructioIdToCover)
import Html exposing (Html, div, img, text)
import Html.Attributes exposing (alt, href, src, style)
import Http
import InstructionHelper exposing (InstructionView, getInstruction)
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), MsgBase, UserRole(..), baseApplication, initBase, updateBase)
import NavBar exposing (instructionsLink, viewNav)
import Utils exposing (existsDecoder, fillParent, flex, flex1, largeCentered, mediumMargin, smallMargin, textCenter, textHtml)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Instruction (WebData InstructionView)
    | NoInstruction



--update


type Msg
    = NoOp
    | GotInstruction (Result Http.Error (Maybe InstructionView))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model navState ->
            let
                authed md =
                    Authorized auth md navState

                updateModel newModel =
                    ( authed newModel, Cmd.none )
            in
            case msg of
                GotInstruction (Ok (Just res)) ->
                    updateModel <| Instruction <| Loaded res

                GotInstruction (Ok Nothing) ->
                    updateModel <| NoInstruction

                GotInstruction (Err err) ->
                    updateModel <| Instruction <| Error

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--view


view : Model -> Html (MsgBase Msg)
view model =
    viewNav model (Just instructionsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        NoInstruction ->
            div largeCentered [ text "There is such instruction" ]

        Instruction ins ->
            viewWebdata ins viewInstruction


viewInstruction : InstructionView -> Html Msg
viewInstruction ins =
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
            [ Button.linkButton [ Button.outlinePrimary, Button.attrs [ href "/instructions" ] ] [ text "Go back" ]
            ]
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd (MsgBase Msg) )
init resp flags =
    let
        insId =
            D.decodeValue (D.field "id" D.int) flags

        initialModel =
            case insId of
                Ok id ->
                    Instruction Loading

                Err _ ->
                    NoInstruction

        initialCmd res =
            case insId of
                Ok id ->
                    getInstruction GotInstruction res.token id

                Err _ ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initialModel initialCmd resp



--subs


subscriptions : Model -> Sub (MsgBase Msg)
subscriptions model =
    Sub.none


main : Program D.Value Model (MsgBase Msg)
main =
    baseApplication
        { init = init
        , view = view
        , update = updateBase update
        , subscriptions = subscriptions
        }
