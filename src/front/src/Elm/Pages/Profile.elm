module Pages.Profile exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.ListGroup as ListGroup
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), IdType(..), getAuthed, historyUrl, postAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style)
import Http
import Json.Decode as D
import Json.Encode as E
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, isAdmin, mapCmd, roleToStr, subscriptionBase, updateBase)
import Main2 exposing (viewBase)
import Utils exposing (SubmittedResult(..), fillParent, flex, flexCenter, largeCentered, mediumMargin, smallMargin, submittedDecoder)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { password : String
    , result : Maybe (WebData SubmittedResult)
    , id : WebData String
    }



--update


type LocalMsg
    = NoOp
    | NewPasswordChanged String
    | ChangePass
    | GotChangePass (Result Http.Error SubmittedResult)
    | GotId (Result Http.Error String)


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
                NewPasswordChanged pass ->
                    updateModel { model | password = pass, result = Nothing }

                ChangePass ->
                    ( authed { model | result = Just Loading }, submitCommand auth.token model.password )

                GotChangePass (Ok res) ->
                    updateModel { model | result = Just <| Loaded res }

                GotChangePass (Err err) ->
                    updateModel { model | result = Just <| Error err }

                GotId id ->
                    let
                        getId =
                            case id of
                                Ok res ->
                                    Loaded res

                                Err err ->
                                    Error err
                    in
                    updateModel { model | id = getId }

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands


getUserId : String -> String -> Cmd Msg
getUserId token login =
    let
        expect =
            Http.expectJson GotId D.string
    in
    getAuthed token (ConvertId <| StringId login) expect Nothing |> mapCmd


submitCommand : String -> String -> Cmd Msg
submitCommand token pass =
    let
        expect =
            Http.expectJson GotChangePass submittedDecoder

        body =
            Http.jsonBody (E.object [ ( "password", E.string pass ) ])
    in
    postAuthed token ChangePassword body expect Nothing |> mapCmd



--view


view : Model -> Html Msg
view model =
    viewBase model Nothing viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    let
        shouldDisableInput =
            case page.result of
                Just Loading ->
                    True

                _ ->
                    False

        historyBtn =
            if isAdmin resp then
                viewWebdata
                    page.id
                    (\id ->
                        Button.linkButton
                            [ Button.outlinePrimary
                            , Button.onClick <| Navigate <| historyUrl "User" id
                            , Button.attrs [ smallMargin ]
                            ]
                            [ text "View history" ]
                    )

            else
                div [] []
    in
    div ([ flex, Flex.row, mediumMargin ] ++ fillParent ++ flexCenter)
        [ div [ flex, Flex.col, class "modal__container" ]
            [ div largeCentered [ text "My Roles" ]
            , ListGroup.ul (List.map (\role -> ListGroup.li [] [ text <| roleToStr role ]) resp.roles)
            , div largeCentered [ text "New Password" ]
            , Input.password
                [ Input.value page.password
                , Input.onInput NewPasswordChanged
                , Input.attrs
                    [ mediumMargin
                    , style "width" "unset"
                    ]
                , Input.disabled shouldDisableInput
                ]
                |> Html.map Main
            , buttonView page |> Html.map Main
            , Button.linkButton [ Button.danger, Button.onClick <| Navigate "/login/new", Button.attrs [ mediumMargin ] ] [ text "Logout" ]
            , historyBtn
            ]
        ]


buttonView page =
    case page.result of
        Just res ->
            viewWebdata res viewResult

        Nothing ->
            Button.button [ Button.primary, Button.attrs [ mediumMargin ], Button.onClick ChangePass ]
                [ text "Change password"
                ]


viewResult : SubmittedResult -> Html msg
viewResult res =
    case res of
        SubmittedSuccess msg cmd ->
            div (largeCentered ++ [ class "text-success" ]) [ text msg ]

        SubmittedFail msg ->
            div (largeCentered ++ [ class "text-warning" ]) [ text msg ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp _ =
    initBase [ Producer, Consumer, Manager ] (View "" Nothing Loading) (\res -> getUserId res.token res.username) resp


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
