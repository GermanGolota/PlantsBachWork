module Pages.AddUser exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), postAuthed)
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, style, value)
import Http
import Json.Decode as D
import Json.Encode as E
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, roleToNumber)
import NavBar exposing (usersLink, viewNav)
import UserRolesSelector exposing (userRolesBtns)
import Utils exposing (SubmittedResult(..), fillParent, flex, flexCenter, largeCentered, mediumMargin, submittedDecoder)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


languages : List String
languages =
    [ "English", "Español", "Українська" ]


type alias View =
    { firstName : String
    , lastName : String
    , login : String
    , phone : String
    , email : String
    , selectedLanguage : String
    , selectedRoles : List UserRole
    , submitResult : Maybe (WebData SubmittedResult)
    }



--update


type Msg
    = NoOp
    | FirstNameChanged String
    | LastNameChanged String
    | EmailChanged String
    | LoginChanged String
    | PhoneChanged String
    | LanguageSelected String
    | RoleSelected UserRole
    | Submit
    | GotSubmit (Result Http.Error SubmittedResult)


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
                FirstNameChanged name ->
                    updateModel { model | firstName = name, submitResult = Nothing }

                LastNameChanged name ->
                    updateModel { model | lastName = name, submitResult = Nothing }

                EmailChanged email ->
                    updateModel { model | email = email, submitResult = Nothing }

                LoginChanged login ->
                    updateModel { model | login = login, submitResult = Nothing }

                PhoneChanged phone ->
                    updateModel { model | phone = phone, submitResult = Nothing }

                LanguageSelected lang ->
                    updateModel { model | selectedLanguage = lang, submitResult = Nothing }

                RoleSelected role ->
                    let
                        newRoles =
                            if List.member role model.selectedRoles then
                                List.filter (\r -> r /= role) model.selectedRoles

                            else
                                model.selectedRoles ++ [ role ]
                    in
                    updateModel { model | selectedRoles = newRoles, submitResult = Nothing }

                Submit ->
                    ( authed { model | submitResult = Just Loading }, createUserCmd auth.token model )

                GotSubmit (Ok res) ->
                    updateModel { model | submitResult = Just (Loaded res) }

                GotSubmit (Err _) ->
                    updateModel { model | submitResult = Just Error }

                NoOp ->
                    noOp

        _ ->
            ( m, Cmd.none )



--commands


createUserCmd : String -> View -> Cmd Msg
createUserCmd token page =
    let
        expect =
            Http.expectJson GotSubmit (submittedDecoder (D.field "success" D.bool) (D.field "message" D.string))
    in
    postAuthed token CreateUser (Http.jsonBody <| encodeBody page) expect Nothing


encodeBody : View -> E.Value
encodeBody page =
    E.object
        [ ( "login", E.string page.login )
        , ( "roles", E.list E.int (List.map roleToNumber page.selectedRoles) )
        , ( "email", E.string page.email )
        , ( "firstName", E.string page.firstName )
        , ( "lastName", E.string page.lastName )
        , ( "phoneNumber", E.string page.phone )
        ]



--view


view : Model -> Html Msg
view model =
    viewNav model (Just usersLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    let
        viewSubmit submit =
            case submit of
                SubmittedSuccess msg ->
                    div [ flex, Flex.row, class "text-success" ] [ text msg ]

                SubmittedFail msg ->
                    div [ flex, Flex.row, class "text-danger" ] [ text msg ]

        submitResult =
            case page.submitResult of
                Just res ->
                    viewWebdata res viewSubmit

                Nothing ->
                    div [] []
    in
    div ([ flex, Flex.row ] ++ fillParent ++ flexCenter)
        [ div [ flex, Flex.col, class "modal__container" ]
            [ div [ flex, Flex.row ]
                (viewInputs page)
            , div [ flex, Flex.row, mediumMargin ]
                [ userRolesBtns RoleSelected page.selectedRoles resp.roles ]
            , submitResult
            , div [ flex, Flex.row, Flex.justifyEnd, mediumMargin ]
                [ viewBtn ]
            ]
        ]


viewInputs : View -> List (Html Msg)
viewInputs page =
    [ div [ flex, Flex.col, mediumMargin ]
        (viewInput "First Name" page.firstName FirstNameChanged
            ++ viewInput "Last Name" page.lastName LastNameChanged
            ++ viewInput "Email" page.email EmailChanged
        )
    , div [ flex, Flex.col, mediumMargin ]
        (viewInput "Login" page.login LoginChanged ++ viewInput "Phone Number" page.phone PhoneChanged ++ viewInputBase "Invite Language" languagesSelector)
    ]


languagesSelector : Html Msg
languagesSelector =
    Select.select [ Select.onChange LanguageSelected ] <| List.map (\lang -> Select.item [ value lang ] [ text lang ]) languages


viewInput : String -> String -> (String -> msg) -> List (Html msg)
viewInput desc val change =
    viewInputBase desc <| Input.text [ Input.value val, Input.onInput change ]


viewInputBase : String -> Html msg -> List (Html msg)
viewInputBase desc input =
    [ div largeCentered [ text desc ], input ]


viewBtn =
    Button.button [ Button.primary, Button.onClick Submit, Button.attrs largeCentered ] [ text "Create and invite" ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialLang =
            Maybe.withDefault "English" <| List.head languages
    in
    initBase [ Producer, Consumer, Manager ] (View "" "" "" "" "" initialLang [] Nothing) (\res -> Cmd.none) resp


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
