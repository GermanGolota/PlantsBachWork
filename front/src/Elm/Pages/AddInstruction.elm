port module Pages.AddInstruction exposing (..)

import Available exposing (Available, availableDecoder)
import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), postAuthed)
import File exposing (File)
import File.Select as FileSelect
import Html exposing (Html, div, text)
import Html.Attributes exposing (href, style, value)
import Html.Parser
import Html.Parser.Util
import Http
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import Multiselect
import NavBar exposing (plantsLink, viewNav)
import Transition exposing (constant)
import Utils exposing (fillParent, flex, flex1, largeCentered, mediumMargin, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--ports


port openEditor : Bool -> Cmd msg


port editorChanged : (String -> msg) -> Sub msg



--model


type alias Model =
    ModelBase View


type alias View =
    { selectedText : String
    , selectedGroupId : Int
    , selectedTitle : String
    , selectedDescription : String
    , uploadedFile : Maybe File
    , available : WebData Available
    , result : Maybe (WebData Int)
    }



--update


type Msg
    = NoOp
    | EditorTextUpdated String
    | GroupSelected Int
    | TitleChanged String
    | DescriptionChanged String
    | OpenEditor
    | GotAvailable (Result Http.Error Available)
    | GotSubmit (Result Http.Error Int)
    | StartUpload
    | ImagesLoaded File (List File)
    | Submit


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
                EditorTextUpdated newText ->
                    ( authed { model | selectedText = newText }, Cmd.none )

                OpenEditor ->
                    ( m, openEditor True )

                StartUpload ->
                    ( m, requestImages )

                ImagesLoaded file _ ->
                    updateModel { model | uploadedFile = Just file }

                TitleChanged title ->
                    updateModel { model | selectedTitle = title }

                DescriptionChanged desc ->
                    updateModel { model | selectedDescription = desc }

                GroupSelected groupId ->
                    updateModel { model | selectedGroupId = groupId }

                GotAvailable (Ok res) ->
                    updateModel { model | available = Loaded res }

                GotAvailable (Err err) ->
                    updateModel { model | available = Error }

                Submit ->
                    ( authed { model | result = Just Loading }, submitCommand auth.token model )

                GotSubmit (Ok res) ->
                    updateModel { model | result = Just (Loaded res) }

                GotSubmit (Err err) ->
                    updateModel { model | result = Just Error }

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands


submitCommand : String -> View -> Cmd Msg
submitCommand token page =
    let
        expect =
            Http.expectJson GotSubmit (D.field "id" D.int)
    in
    postAuthed token CreateInstruction (bodyEncoder page) expect Nothing


bodyEncoder : View -> Http.Body
bodyEncoder page =
    let
        constant =
            [ Http.stringPart "GroupId" <| String.fromInt page.selectedGroupId
            , Http.stringPart "Text" page.selectedText
            , Http.stringPart "Title" page.selectedTitle
            , Http.stringPart "Description" page.selectedDescription
            ]

        parts =
            case page.uploadedFile of
                Just file ->
                    constant ++ [ Http.filePart "file" file ]

                Nothing ->
                    constant
    in
    Http.multipartBody parts


getAvailable : String -> Cmd Msg
getAvailable token =
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing


requestImages : Cmd Msg
requestImages =
    FileSelect.files [ "image/png", "image/jpg" ] ImagesLoaded



--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    viewWebdata page.available (viewMain page)


viewMain : View -> Available -> Html Msg
viewMain page av =
    let
        viewRow =
            div [ flex, Flex.row, flex1, smallMargin ]

        viewCol =
            div [ flex1, Flex.col, flex, smallMargin ]

        changeFunc str =
            case String.toInt str of
                Just res ->
                    GroupSelected res

                Nothing ->
                    NoOp

        groups =
            Multiselect.getValues av.groups

        viewGroup group =
            Select.item [ value <| Tuple.first group ] [ text <| Tuple.second group ]

        fileStr =
            case page.uploadedFile of
                Just file ->
                    File.name file

                Nothing ->
                    "No file selected"

        viewResult result =
            viewRow
                [ viewCol
                    [ div largeCentered [ text "Successfully created instruction!" ]
                    , Button.linkButton [ Button.primary, Button.attrs [ href <| "/instructions/" ++ String.fromInt result ] ] [ text "Open Instruction" ]
                    ]
                ]

        resultRow =
            case page.result of
                Just res ->
                    viewWebdata res viewResult

                Nothing ->
                    div [] []
    in
    div ([ Flex.col, flex ] ++ fillParent)
        [ viewRow
            [ viewCol
                [ div largeCentered [ text "Group" ]
                , Select.select [ Select.onChange changeFunc ] (List.map viewGroup groups)
                ]
            ]
        , viewRow
            [ viewCol
                [ div largeCentered [ text "Title" ]
                , Input.text [ Input.value page.selectedTitle, Input.onInput TitleChanged ]
                ]
            , viewCol
                [ div largeCentered [ text "Description" ]
                , Input.text [ Input.value page.selectedDescription, Input.onInput DescriptionChanged ]
                ]
            ]
        , viewRow
            [ Button.button
                [ Button.primary, Button.onClick StartUpload, Button.attrs [ flex1 ] ]
                [ text "Upload" ]
            , viewCol [ div largeCentered [ text fileStr ] ]
            ]
        , viewRow
            [ Button.button
                [ Button.primary, Button.onClick OpenEditor, Button.attrs [ flex1 ] ]
                [ text "Edit text" ]
            ]
        , div [ flex, Flex.row, style "flex" "4" ]
            [ viewCol
                [ div largeCentered [ text "Instruction Content" ]
                , div ([ style "border" "1px solid gray" ] ++ fillParent) [ Html.p [] (textHtml page.selectedText) ]
                ]
            ]
        , resultRow
        , viewRow
            [ Button.button
                [ Button.primary, Button.onClick Submit, Button.attrs [ flex1, mediumMargin ] ]
                [ text "Create" ]
            ]
        ]


textHtml : String -> List (Html.Html msg)
textHtml t =
    case Html.Parser.run t of
        Ok nodes ->
            Html.Parser.Util.toVirtualDom nodes

        Err _ ->
            []



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    initBase [ Producer, Consumer, Manager ] (View "" 1 "" "" Nothing Loading Nothing) (\res -> getAvailable res.token) resp



--subs


subscriptions : Model -> Sub Msg
subscriptions model =
    editorChanged EditorTextUpdated


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
