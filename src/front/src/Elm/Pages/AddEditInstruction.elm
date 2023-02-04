port module Pages.AddEditInstruction exposing (..)

import Available exposing (Available, availableDecoder)
import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), historyUrl, postAuthed)
import File exposing (File)
import File.Select as FileSelect
import Html exposing (Html, div, text)
import Html.Attributes exposing (style, value)
import Http
import InstructionHelper exposing (InstructionView, getInstruction)
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, mapCmd, mapSub, updateBase)
import Multiselect
import NavBar exposing (instructionsLink, viewNav)
import Transition exposing (constant)
import Utils exposing (decodeId, fillParent, flex, flex1, largeCentered, mediumMargin, smallMargin, textHtml)
import Webdata exposing (WebData(..), viewWebdata)



--ports


port openEditor : String -> Cmd msg


port editorChanged : (String -> msg) -> Sub msg



--model


type alias Model =
    ModelBase ViewType


type ViewType
    = Add View
    | Edit String (WebData View)
    | BadEdit


type alias View =
    { selectedText : String
    , selectedGroupName : String
    , selectedTitle : String
    , selectedDescription : String
    , uploadedFile : Maybe File
    , available : WebData Available
    , result : Maybe (WebData String)
    }



--update


type LocalMsg
    = NoOp
    | GotInstruction (Result Http.Error (Maybe InstructionView))
    | EditorTextUpdated String
    | GroupSelected String
    | TitleChanged String
    | DescriptionChanged String
    | OpenEditor
    | GotAvailable (Result Http.Error Available)
    | GotSubmit (Result Http.Error String)
    | StartUpload
    | ImagesLoaded File (List File)
    | Submit


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
                    case model of
                        Edit id (Loaded oldModel) ->
                            ( authed <| Edit id (Loaded newModel), Cmd.none )

                        Add oldModel ->
                            ( authed <| Add newModel, Cmd.none )

                        _ ->
                            noOp

                getMainView =
                    case model of
                        Edit id (Loaded oldModel) ->
                            oldModel

                        Add oldModel ->
                            oldModel

                        _ ->
                            emptyView
            in
            case msg of
                EditorTextUpdated newText ->
                    updateModel { getMainView | selectedText = newText }

                OpenEditor ->
                    ( m, openEditor getMainView.selectedText )

                StartUpload ->
                    ( m, requestImages )

                GotInstruction (Ok res) ->
                    case model of
                        Edit id _ ->
                            case res of
                                Just ins ->
                                    ( authed <| Edit id <| Loaded (convertView ins), getAvailable auth.token )

                                Nothing ->
                                    ( authed <| BadEdit, Cmd.none )

                        _ ->
                            noOp

                GotInstruction (Err err) ->
                    case model of
                        Edit id _ ->
                            ( authed <| Edit id <| Error err, Cmd.none )

                        _ ->
                            noOp

                ImagesLoaded file _ ->
                    updateModel { getMainView | uploadedFile = Just file }

                TitleChanged title ->
                    updateModel { getMainView | selectedTitle = title }

                DescriptionChanged desc ->
                    updateModel { getMainView | selectedDescription = desc }

                GroupSelected groupName ->
                    updateModel { getMainView | selectedGroupName = groupName }

                GotAvailable (Ok res) ->
                    updateModel { getMainView | available = Loaded res, selectedGroupName = res.groups |> Multiselect.getValues |> List.head |> Maybe.withDefault ( "", "" ) |> Tuple.first }

                GotAvailable (Err err) ->
                    updateModel { getMainView | available = Error err }

                Submit ->
                    let
                        updatedResult =
                            Tuple.first <| updateModel { getMainView | result = Just Loading }
                    in
                    case model of
                        Add page ->
                            ( updatedResult, submitAddCommand auth.token page )

                        Edit id (Loaded page) ->
                            ( updatedResult, submitEditCommand auth.token id page )

                        _ ->
                            noOp

                GotSubmit (Ok res) ->
                    updateModel { getMainView | result = Just (Loaded res) }

                GotSubmit (Err err) ->
                    updateModel { getMainView | result = Just <| Error err }

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )


convertView : InstructionView -> View
convertView ins =
    View ins.text ins.groupId ins.title ins.description Nothing Loading Nothing



--commands


submitAddCommand : String -> View -> Cmd Msg
submitAddCommand token page =
    let
        expect =
            Http.expectJson GotSubmit decodeId
    in
    postAuthed token CreateInstruction (bodyEncoder page) expect Nothing |> mapCmd


submitEditCommand : String -> String -> View -> Cmd Msg
submitEditCommand token id page =
    let
        expect =
            Http.expectJson GotSubmit decodeId
    in
    postAuthed token (EditInstruction id) (bodyEncoder page) expect Nothing |> mapCmd


bodyEncoder : View -> Http.Body
bodyEncoder page =
    let
        constant =
            [ Http.stringPart "GroupName" page.selectedGroupName
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
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing |> mapCmd


requestImages : Cmd Msg
requestImages =
    FileSelect.files [ "image/png", "image/jpg" ] ImagesLoaded |> mapCmd



--view


view : Model -> Html Msg
view model =
    viewNav model (Just instructionsLink) viewPage


viewPage : AuthResponse -> ViewType -> Html Msg
viewPage resp page =
    case page of
        Add add ->
            viewWebdata add.available (viewMain (div [] []) False add)

        Edit id edit ->
            let
                btn =
                    Button.linkButton
                        [ Button.outlinePrimary
                        , Button.onClick <| Navigate <| historyUrl "PlantInstruction" id
                        , Button.attrs [ smallMargin ]
                        ]
                        [ text "View history" ]
            in
            viewWebdata edit (\editLoaded -> viewWebdata editLoaded.available (viewMain btn True editLoaded))

        BadEdit ->
            div largeCentered [ text "There is not such instruction!" ]


viewMain : Html Msg -> Bool -> View -> Available -> Html Msg
viewMain historyBtn isEdit page av =
    let
        viewRow =
            div [ flex, Flex.row, flex1, smallMargin ]

        viewCol =
            div [ flex1, Flex.col, flex, smallMargin ]

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

        resultText =
            if isEdit then
                "Successfully edited instruction!"

            else
                "Successfully created instruction!"

        viewResult result =
            viewRow
                [ viewCol
                    [ div largeCentered [ text resultText ]
                    , Button.linkButton [ Button.primary, Button.onClick <| Navigate ("/instructions/" ++ result) ] [ text "Open Instruction" ]
                    ]
                ]

        resultRow =
            case page.result of
                Just res ->
                    case res of
                        Loading ->
                            [ viewWebdata res viewResult ]

                        _ ->
                            [ viewWebdata res viewResult
                            , viewRow [ Button.button [ Button.primary, Button.onClick <| Main Submit, Button.attrs [ flex1, mediumMargin ] ] [ text createText ] ]
                            , historyBtn
                            ]

                Nothing ->
                    [ viewRow
                        [ Button.button [ Button.primary, Button.onClick <| Main Submit, Button.attrs [ flex1, mediumMargin ] ] [ text createText ]
                        , historyBtn
                        ]
                    ]

        createText =
            if isEdit then
                "Save Changes"

            else
                "Create"
    in
    div ([ Flex.col, flex ] ++ fillParent)
        ([ viewRow
            [ viewCol
                [ div largeCentered [ text "Group" ]
                , Select.select [ Select.onChange (\str -> Main <| GroupSelected str) ] (List.map viewGroup groups)
                ]
            ]
         , viewRow
            [ viewCol
                [ div largeCentered [ text "Title" ]
                , Input.text [ Input.value page.selectedTitle, Input.onInput (\str -> Main <| TitleChanged str) ]
                ]
            , viewCol
                [ div largeCentered [ text "Description" ]
                , Input.text [ Input.value page.selectedDescription, Input.onInput (\str -> Main <| DescriptionChanged str) ]
                ]
            ]
         , viewRow
            [ Button.button
                [ Button.primary, Button.onClick <| Main StartUpload, Button.attrs [ flex1 ] ]
                [ text "Upload" ]
            , viewCol [ div largeCentered [ text fileStr ] ]
            ]
         , viewRow
            [ Button.button
                [ Button.primary, Button.onClick <| Main OpenEditor, Button.attrs [ flex1 ] ]
                [ text "Edit text" ]
            ]
         , div [ flex, Flex.row, style "flex" "4" ]
            [ viewCol
                [ div largeCentered [ text "Instruction Content" ]
                , div (fillParent ++ [ style "border" "1px solid gray" ]) [ Html.p [] (textHtml page.selectedText) ]
                ]
            ]
         ]
            ++ resultRow
        )



--init


emptyView =
    View "" "1" "" "" Nothing Loading Nothing


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialModel =
            case D.decodeValue (D.field "isEdit" D.bool) flags of
                Ok isEdit ->
                    if isEdit then
                        Edit (Result.withDefault "-1" (D.decodeValue (D.field "id" decodeId) flags)) Loading

                    else
                        Add emptyView

                Err err ->
                    Add emptyView

        initialCmd res =
            case initialModel of
                Edit id _ ->
                    getInstruction GotInstruction res.token id |> mapCmd

                Add _ ->
                    getAvailable res.token

                BadEdit ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initialModel initialCmd resp



--subs


subscriptions : Model -> Sub Msg
subscriptions model =
    editorChanged EditorTextUpdated |> mapSub


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
