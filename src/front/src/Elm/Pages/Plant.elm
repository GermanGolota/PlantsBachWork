module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Endpoints exposing (Endpoint(..), getAuthed, historyUrl, postAuthed)
import Html exposing (Html, div, i, input, text)
import Html.Attributes exposing (checked, class, disabled, href, style, type_, value)
import Http
import ImageList as ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (required)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, isAdmin, mapCmd, updateBase)
import Maybe exposing (map)
import NavBar exposing (searchLink, viewNav)
import PlantHelper exposing (PlantModel, plantDecoder, viewDesc, viewPlantBase, viewPlantLeft)
import Utils exposing (SubmittedResult(..), fillParent, flex, flex1, largeCentered, largeFont, mediumMargin, smallMargin, submittedDecoder)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant PlantView


type alias PlantView =
    { id : String
    , plantType : ViewType
    }


type ViewType
    = JustPlant (WebData (Maybe PlantModel))
    | Order OrderView


type alias OrderView =
    { plant : WebData (Maybe PlantModel)
    , addresses : WebData (List DeliveryAddress)
    , selected : SelectedAddress
    , result : Maybe (WebData SubmittedResult)
    }


type SelectedAddress
    = None
    | City String
    | Location Int
    | Selected Bool DeliveryAddress


type alias DeliveryAddress =
    { city : String
    , location : Int
    }



--update


type LocalMsg
    = NoOp
    | GotPlant (Result Http.Error (Maybe PlantModel))
    | GotAddresses (Result Http.Error (List DeliveryAddress))
    | Images ImageList.Msg
    | AddressSelected String Int
    | CityChanged String
    | LocationChanged Int
    | Submit
    | GotSubmit (Result Http.Error SubmittedResult)


type alias Msg =
    MsgBase LocalMsg


update : Msg -> Model -> ( Model, Cmd Msg )
update =
    updateBase localUpdate


localUpdate : LocalMsg -> Model -> ( Model, Cmd Msg )
localUpdate msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed newModel =
                    Authorized auth newModel

                authedPlant plantView =
                    authed <| Plant plantView

                authedOrder p ord =
                    authedPlant { p | plantType = ord }
            in
            case ( msg, model ) of
                ( GotPlant (Ok res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Loaded res }, Cmd.none )

                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order { orderView | plant = Loaded res } }, Cmd.none )

                ( GotPlant (Err err), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Error err }, Cmd.none )

                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order { orderView | plant = Error err } }, Cmd.none )

                ( GotAddresses (Ok res), Plant p ) ->
                    let
                        sel =
                            case List.head res of
                                Just val ->
                                    Selected True val

                                Nothing ->
                                    None
                    in
                    case p.plantType of
                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order <| { orderView | addresses = Loaded res, selected = sel } }, Cmd.none )

                        _ ->
                            noOp

                ( GotAddresses (Err err), Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order <| { orderView | plant = Error err } }, Cmd.none )

                        _ ->
                            noOp

                ( Images img, Plant p ) ->
                    case p.plantType of
                        JustPlant (Loaded (Just pl)) ->
                            ( authedPlant { p | plantType = JustPlant <| Loaded <| Just { pl | images = ImageList.update img pl.images } }, Cmd.none )

                        Order orderView ->
                            case orderView.plant of
                                Loaded (Just pl) ->
                                    ( authedPlant { p | plantType = Order { orderView | plant = Loaded <| Just { pl | images = ImageList.update img pl.images } } }, Cmd.none )

                                _ ->
                                    noOp

                        _ ->
                            noOp

                ( AddressSelected city location, Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedOrder p (Order <| { orderView | selected = Selected True <| DeliveryAddress city location }), Cmd.none )

                        _ ->
                            noOp

                ( CityChanged city, Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            let
                                updateSelected selected =
                                    ( authedOrder p (Order <| { orderView | selected = selected }), Cmd.none )
                            in
                            case orderView.selected of
                                Selected _ addr ->
                                    updateSelected <| Selected False (DeliveryAddress city addr.location)

                                Location loc ->
                                    updateSelected <| Selected False <| DeliveryAddress city loc

                                City _ ->
                                    updateSelected <| City city

                                None ->
                                    updateSelected <| City city

                        _ ->
                            noOp

                ( LocationChanged loc, Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            let
                                updateSelected selected =
                                    ( authedOrder p (Order <| { orderView | selected = selected }), Cmd.none )
                            in
                            case orderView.selected of
                                Selected _ addr ->
                                    updateSelected <| Selected False (DeliveryAddress addr.city loc)

                                City city ->
                                    updateSelected <| Selected False <| DeliveryAddress city loc

                                Location _ ->
                                    updateSelected <| Location loc

                                None ->
                                    updateSelected <| Location loc

                        _ ->
                            noOp

                ( Submit, Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            case orderView.selected of
                                Selected _ addr ->
                                    ( authedOrder p <| Order { orderView | result = Just Loading }, submitCmd auth.token p.id addr.city addr.location )

                                _ ->
                                    noOp

                        _ ->
                            noOp

                ( GotSubmit (Ok res), Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedOrder p <| Order { orderView | result = Just <| Loaded res }, Cmd.none )

                        _ ->
                            noOp

                ( GotSubmit (Err err), Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedOrder p <| Order { orderView | result = Just <| Error err }, Cmd.none )

                        _ ->
                            noOp

                ( _, _ ) ->
                    noOp

        _ ->
            noOp



--commands


submitCmd : String -> String -> String -> Int -> Cmd Msg
submitCmd token plantId city mailNumber =
    let
        expect =
            Http.expectJson GotSubmit submittedDecoder
    in
    postAuthed token (OrderPost plantId city mailNumber) Http.emptyBody expect Nothing |> mapCmd


getAddressesCommand : String -> Cmd Msg
getAddressesCommand token =
    let
        expect =
            Http.expectJson GotAddresses addressesDecoder
    in
    getAuthed token Addresses expect Nothing |> mapCmd


addressesDecoder : D.Decoder (List DeliveryAddress)
addressesDecoder =
    D.field "addresses" (D.list addressDecoder)


addressDecoder : D.Decoder DeliveryAddress
addressDecoder =
    D.succeed DeliveryAddress
        |> required "city" D.string
        |> required "mailNumber" D.int


getPlantCommand : String -> String -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder Nothing token)
    in
    getAuthed token (Post plantId) expect Nothing |> mapCmd



--view


view : Model -> Html Msg
view model =
    viewNav model (Just searchLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage auth page =
    let
        allowOrder =
            List.member Consumer auth.roles
    in
    case page of
        NoPlant ->
            div [] [ text "Please select a plant", Button.linkButton [ Button.primary, Button.onClick <| Navigate "/search", Button.attrs [ smallMargin ] ] [ text "Return to search" ] ]

        Plant p ->
            let
                plantView =
                    viewPlantFull p.id

                orderView =
                    viewOrderFull (isAdmin auth) allowOrder p.id
            in
            case p.plantType of
                JustPlant pl ->
                    viewWebdata pl (plantView <| viewPlant (isAdmin auth) allowOrder)

                Order { plant, addresses, selected, result } ->
                    viewWebdata plant (orderView selected addresses result)


viewOrderFull : Bool -> Bool -> String -> SelectedAddress -> WebData (List DeliveryAddress) -> Maybe (WebData SubmittedResult) -> Maybe PlantModel -> Html Msg
viewOrderFull isAdmin allowOrder id selected del result p =
    viewPlantFull id (viewOrder isAdmin allowOrder selected del result) p


viewOrder : Bool -> Bool -> SelectedAddress -> WebData (List DeliveryAddress) -> Maybe (WebData SubmittedResult) -> String -> PlantModel -> Html Msg
viewOrder isAdmin allowOrder selected del result id pl =
    let
        header textT =
            div largeCentered [ text textT ]

        resultView =
            case result of
                Just res ->
                    case res of
                        Loading ->
                            [ viewWebdata res viewResult ]

                        _ ->
                            [ viewWebdata res viewResult, interactionButtons isAdmin allowOrder True id ]

                Nothing ->
                    [ interactionButtons isAdmin allowOrder True id ]
    in
    div (fillParent ++ [ flex, Flex.row ])
        [ viewPlantLeft (\msg -> Main <| Images msg) pl
        , div [ flex, Flex.col, flex1 ]
            (viewDesc False (\str -> Main NoOp) pl
                ++ ([ header "Payment methods"
                    , customRadio True "Pay now" False
                    , customRadio False "Pay on arrival" True
                    , viewWebdata del (viewLocation selected) |> Html.map Main
                    ]
                        ++ resultView
                   )
            )
        ]


customRadio : Bool -> String -> Bool -> Html msg
customRadio isDisabled msg isChecked =
    div [ flex1, flex, Flex.row, Flex.alignItemsCenter ]
        [ input [ type_ "radio", disabled isDisabled, checked isChecked ] []
        , div [ largeFont, mediumMargin ] [ text msg ]
        ]


viewResult : SubmittedResult -> Html Msg
viewResult result =
    let
        baseView className message =
            div [ flex1 ] [ div [ largeFont, class className ] [ text message ] ]

        viewText =
            case result of
                SubmittedSuccess msg ->
                    baseView "text-primary" msg

                SubmittedFail msg ->
                    baseView "text-warning" msg
    in
    case result of
        SubmittedSuccess msg ->
            div [ flex, Flex.col, flex1 ]
                [ div [ Flex.row, flex1 ]
                    [ viewText ]
                , div [ Flex.row, flex1 ]
                    [ Button.linkButton [ Button.onClick <| Navigate "/orders", Button.info, Button.attrs [ largeFont ] ] [ text "View my orders" ]
                    ]
                ]

        SubmittedFail msg ->
            div [ flex1 ]
                [ viewText
                ]


viewLocation : SelectedAddress -> List DeliveryAddress -> Html LocalMsg
viewLocation sel dels =
    let
        valSep =
            "___"

        viewDel delAddr =
            Select.item [ value (delAddr.city ++ valSep ++ String.fromInt delAddr.location) ] [ text (delAddr.city ++ " | " ++ locationToString delAddr.location) ]

        getAddressFromValue value =
            case String.split valSep value of
                [ city, location ] ->
                    map (DeliveryAddress city) (String.toInt location)

                _ ->
                    Nothing

        delAddressToEvent addr =
            case addr of
                Just address ->
                    AddressSelected address.city address.location

                Nothing ->
                    NoOp

        selectClass =
            case sel of
                Selected True _ ->
                    ""

                _ ->
                    "bg-secondary"
    in
    div (fillParent ++ [ flex, Flex.col, mediumMargin, flex1 ])
        ([ div largeCentered [ text "Previous" ]
         , Select.select [ Select.onChange (\val -> delAddressToEvent <| getAddressFromValue val), Select.attrs [ class selectClass ] ]
            (List.map viewDel dels)
         ]
            ++ viewSelected sel
        )


locationToString : Int -> String
locationToString location =
    String.fromInt location ++ ", Nova Poshta Delivery Address"


viewSelected : SelectedAddress -> List (Html LocalMsg)
viewSelected selected =
    let
        cityText =
            case selected of
                Selected _ addr ->
                    addr.city

                City city ->
                    city

                _ ->
                    ""

        locationText =
            case selected of
                Selected _ addr ->
                    String.fromInt addr.location

                Location location ->
                    String.fromInt location

                _ ->
                    ""

        locationFromStr str =
            case String.toInt str of
                Just location ->
                    LocationChanged location

                Nothing ->
                    NoOp
    in
    [ div largeCentered [ text "City" ]
    , div [ flex, flex1, Flex.row, Flex.alignItemsCenter ]
        [ i [ class "fa-solid fa-location-dot", smallMargin ] []
        , Input.text [ Input.value cityText, Input.onInput CityChanged ]
        ]
    , div largeCentered [ text "Location" ]
    , Input.number [ Input.attrs [ flex1 ], Input.value locationText, Input.onInput locationFromStr ]
    ]


viewPlantFull : String -> (String -> PlantModel -> Html Msg) -> Maybe PlantModel -> Html Msg
viewPlantFull id viewFunc p =
    case p of
        Just plant ->
            viewFunc id plant

        Nothing ->
            div [] [ text "This plant is no longer available, sorry" ]


viewPlant : Bool -> Bool -> String -> PlantModel -> Html Msg
viewPlant isAdmin allowOrder id plant =
    viewPlantBase False (\str -> Main NoOp) (\msg -> Main <| Images msg) (interactionButtons isAdmin allowOrder False id) plant


interactionButtons : Bool -> Bool -> Bool -> String -> Html Msg
interactionButtons isAdmin allowOrder isOrder id =
    let
        backUrl =
            if isOrder then
                "/plant/" ++ id

            else
                "/search"

        backNavigate =
            if isOrder then
                []

            else
                [ Button.onClick <| Navigate backUrl ]

        orderText =
            if isOrder then
                "Confirm Order"

            else
                "Order"

        orderUrl =
            if isOrder then
                "#"

            else
                "/plant/" ++ id ++ "/order"

        orderOnClick =
            if isOrder then
                Button.onClick <| Main Submit

            else
                --fix issue with plants component not reloading
                Button.attrs []

        --Button.onClick <| Navigate orderUrl
        orderBtn =
            if allowOrder then
                Button.linkButton
                    [ Button.primary
                    , orderOnClick
                    , Button.onClick <| Navigate orderUrl
                    , Button.attrs [ smallMargin, largeFont ]
                    ]
                    [ text orderText ]

            else
                div [] []

        historyBtn =
            if isOrder == False && isAdmin then
                Button.linkButton
                    [ Button.outlinePrimary
                    , Button.onClick <| Navigate <| historyUrl "PlantPost" id
                    , Button.attrs [ smallMargin, largeFont ]
                    ]
                    [ text "View history" ]

            else
                div [] []
    in
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton
            ([ Button.primary
             , Button.onClick <| Navigate <| backUrl
             , Button.attrs [ smallMargin, largeFont ]
             ]
                ++ backNavigate
            )
            [ text "Back" ]
        , orderBtn
        , historyBtn
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialModel =
            decodeInitial flags

        cmds authResp =
            case initialModel of
                Plant p ->
                    case p.plantType of
                        JustPlant _ ->
                            getPlantCommand authResp.token p.id

                        Order _ ->
                            Cmd.batch [ getPlantCommand authResp.token p.id, getAddressesCommand authResp.token ]

                NoPlant ->
                    Cmd.none
    in
    initBase
        [ Producer, Consumer, Manager ]
        initialModel
        cmds
        resp


decodeInitial : D.Value -> View
decodeInitial flags =
    let
        isOrder =
            decodeIsOrder flags
    in
    case decodePlantId flags of
        Err _ ->
            NoPlant

        Ok plantId ->
            if isOrder then
                Plant (PlantView plantId <| Order <| OrderView Loading Loading None Nothing)

            else
                Plant (PlantView plantId <| JustPlant Loading)


decodeIsOrder : D.Value -> Bool
decodeIsOrder flags =
    let
        decoded =
            D.decodeValue (D.field "isOrder" D.bool) flags
    in
    case decoded of
        Ok res ->
            res

        Err _ ->
            False


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


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
