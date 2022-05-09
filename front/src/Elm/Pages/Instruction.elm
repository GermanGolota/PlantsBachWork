module Pages.Instruction exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed, instructioIdToCover)
import Html exposing (Html, div, img, text)
import Html.Attributes exposing (alt, href, src, style)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import NavBar exposing (instructionsLink, viewNav)
import Utils exposing (existsDecoder, fillParent, flex, flex1, largeCentered, mediumMargin, smallMargin, textCenter, textHtml)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Instruction (WebData InstructionView)
    | NoInstruction


type alias InstructionView =
    { id : Int
    , title : String
    , description : String
    , imageUrl : Maybe String
    , text : String
    }



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
                    updateModel <| Instruction <| Error

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands


getInstruction : String -> Int -> Cmd Msg
getInstruction token id =
    let
        expect =
            Http.expectJson GotInstruction (decodeInstruction token)
    in
    getAuthed token (GetInstruction id) expect Nothing


decodeInstruction : String -> D.Decoder (Maybe InstructionView)
decodeInstruction token =
    existsDecoder (decodeInstructionBase token)


decodeInstructionBase : String -> D.Decoder InstructionView
decodeInstructionBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed InstructionView
        |> requiredItem "id" D.int
        |> requiredItem "title" D.string
        |> requiredItem "description" D.string
        |> custom (coverDecoder token)
        |> requiredItem "instructionText" D.string


coverDecoder : String -> D.Decoder (Maybe String)
coverDecoder token =
    D.at [ "item", "hasCover" ] D.bool |> D.andThen (coverImageDecoder token)


coverImageDecoder token hasCover =
    if hasCover then
        D.map (\id -> Just (instructioIdToCover token id)) (D.at [ "item", "id" ] D.int)

    else
        D.succeed Nothing



--view


view : Model -> Html Msg
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


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
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
                    getInstruction res.token id

                Err _ ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initialModel initialCmd resp



--subs


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
