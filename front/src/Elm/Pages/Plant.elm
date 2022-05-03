module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Radio as Radio
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed, imageIdToUrl, postAuthed)
import Html exposing (Html, div, i, input, text)
import Html.Attributes exposing (checked, class, disabled, href, style, type_, value)
import Http
import ImageList as ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import Maybe exposing (map)
import NavBar exposing (searchLink, viewNav)
import Utils exposing (fillParent, flex, flex1, formatPrice, largeCentered, largeFont, mediumFont, mediumMargin, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant PlantView


type alias PlantView =
    { id : Int
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


type SubmittedResult
    = SubmittedSuccess String
    | SubmittedFail String


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


type alias PlantModel =
    { name : String
    , description : String
    , price : Float
    , soil : String
    , regions : List String
    , group : String

    --{createdDate}({createdHumanDate})
    , created : String
    , sellerName : String
    , sellerPhone : String
    , sellerCreds : PersonCreds
    , caretakerCreds : PersonCreds
    , images : ImageList.Model
    }


type alias PersonCreds =
    { sold : Int
    , cared : Int
    , instructions : Int
    }


type Msg
    = NoOp
    | GotPlant (Result Http.Error (Maybe PlantModel))
    | GotAddresses (Result Http.Error (List DeliveryAddress))
    | Images ImageList.Msg
    | AddressSelected String Int
    | CityChanged String
    | LocationChanged Int
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

                ( GotPlant (Err res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Error }, Cmd.none )

                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order { orderView | plant = Error } }, Cmd.none )

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

                ( GotAddresses (Err res), Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedPlant <| { p | plantType = Order <| { orderView | plant = Error } }, Cmd.none )

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

                ( GotSubmit (Err res), Plant p ) ->
                    case p.plantType of
                        Order orderView ->
                            ( authedOrder p <| Order { orderView | result = Just Error }, Cmd.none )

                        _ ->
                            noOp

                ( _, _ ) ->
                    noOp

        _ ->
            noOp



--commands


submitCmd : String -> Int -> String -> Int -> Cmd Msg
submitCmd token plantId city mailNumber =
    let
        expect =
            Http.expectJson GotSubmit submittedDecoder
    in
    postAuthed token (OrderPost plantId city mailNumber) Http.emptyBody expect Nothing


submittedDecoder : D.Decoder SubmittedResult
submittedDecoder =
    D.field "successfull" D.bool |> D.andThen submittedMsgDecoder


submittedMsgDecoder : Bool -> D.Decoder SubmittedResult
submittedMsgDecoder success =
    let
        getMsg =
            D.field "message" D.string
    in
    if success then
        D.map SubmittedSuccess getMsg

    else
        D.map SubmittedFail getMsg


getAddressesCommand : String -> Cmd Msg
getAddressesCommand token =
    let
        expect =
            Http.expectJson GotAddresses addressesDecoder
    in
    getAuthed token Addresses expect Nothing


addressesDecoder : D.Decoder (List DeliveryAddress)
addressesDecoder =
    D.field "addresses" (D.list addressDecoder)


addressDecoder : D.Decoder DeliveryAddress
addressDecoder =
    D.succeed DeliveryAddress
        |> required "city" D.string
        |> required "mailNumber" D.int


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder token)
    in
    getAuthed token (Post plantId) expect Nothing


plantDecoder : String -> D.Decoder (Maybe PlantModel)
plantDecoder token =
    let
        initedDecoder =
            plantItemDecoder token
    in
    D.field "exists" D.bool |> D.andThen initedDecoder


plantItemDecoder : String -> Bool -> D.Decoder (Maybe PlantModel)
plantItemDecoder token exists =
    if exists then
        D.map Just (plantDecoderBase token)

    else
        D.succeed Nothing


plantDecoderBase : String -> D.Decoder PlantModel
plantDecoderBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed PlantModel
        |> requiredItem "plantName" D.string
        |> requiredItem "description" D.string
        |> requiredItem "price" D.float
        |> requiredItem "soilName" D.string
        |> requiredItem "regions" (D.list D.string)
        |> requiredItem "groupName" D.string
        |> custom createdDecoder
        |> requiredItem "sellerName" D.string
        |> requiredItem "sellerPhone" D.string
        |> custom (credsDecoder "seller")
        |> custom (credsDecoder "careTaker")
        |> custom (imagesDecoder token)


imagesDecoder : String -> D.Decoder ImageList.Model
imagesDecoder token =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.at [ "item", "images" ] (D.list D.int))


imageIdsToModel : String -> List Int -> ImageList.Model
imageIdsToModel token ids =
    let
        baseList =
            List.map (\id -> ( id, imageIdToUrl token id )) ids
    in
    ImageList.fromDict <| Dict.fromList baseList


credsDecoder : String -> D.Decoder PersonCreds
credsDecoder person =
    let
        combine str =
            person ++ str

        requiredItem val =
            requiredAt [ "item", val ]
    in
    D.succeed PersonCreds
        |> requiredItem (combine "Cared") D.int
        |> requiredItem (combine "Sold") D.int
        |> requiredItem (combine "Instructions") D.int


createdDecoder : D.Decoder String
createdDecoder =
    let
        combine date humanDate =
            date ++ " (" ++ humanDate ++ ")"
    in
    D.map2
        combine
        (D.at [ "item", "createdDate" ] D.string)
        (D.at [ "item", "createdHumanDate" ] D.string)



--view


view : Model -> Html Msg
view model =
    viewNav model (Just searchLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage _ page =
    case page of
        NoPlant ->
            div [] [ text "Please select a plant", Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href "/search" ] ] [ text "Return to search" ] ]

        Plant p ->
            let
                plantView =
                    viewPlantFull p.id

                orderView =
                    viewOrderFull p.id
            in
            case p.plantType of
                JustPlant pl ->
                    viewWebdata pl (plantView viewPlant)

                Order { plant, addresses, selected, result } ->
                    viewWebdata plant (orderView selected addresses result)


viewOrderFull : Int -> SelectedAddress -> WebData (List DeliveryAddress) -> Maybe (WebData SubmittedResult) -> Maybe PlantModel -> Html Msg
viewOrderFull id selected del result p =
    viewPlantFull id (viewOrder selected del result) p


viewOrder : SelectedAddress -> WebData (List DeliveryAddress) -> Maybe (WebData SubmittedResult) -> Int -> PlantModel -> Html Msg
viewOrder selected del result id pl =
    let
        header textT =
            div largeCentered [ text textT ]

        resultView =
            case result of
                Just res ->
                    viewWebdata res viewResult

                Nothing ->
                    div [] []
    in
    div (fillParent ++ [ flex, Flex.row ])
        [ viewPlantLeft pl
        , div [ flex, Flex.col, flex1 ]
            (viewDesc pl
                ++ [ header "Payment methods"
                   , customRadio True "Pay now" False
                   , customRadio False "Pay on arrival" True
                   , viewWebdata del (viewLocation selected)
                   , resultView
                   , interactionButtons True id
                   ]
            )
        ]


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
    in
    case result of
        SubmittedSuccess msg ->
            baseView "bg-primary" msg

        SubmittedFail msg ->
            baseView "bg-warning" msg


viewLocation : SelectedAddress -> List DeliveryAddress -> Html Msg
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


viewSelected : SelectedAddress -> List (Html Msg)
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


viewPlantFull : Int -> (Int -> PlantModel -> Html Msg) -> Maybe PlantModel -> Html Msg
viewPlantFull id viewFunc p =
    case p of
        Just plant ->
            viewFunc id plant

        Nothing ->
            div [] [ text "This plant is no longer available, sorry" ]


viewPlant : Int -> PlantModel -> Html Msg
viewPlant id plant =
    let
        filled args =
            fillParent ++ args
    in
    div (filled [ flex, Flex.row ])
        [ viewPlantLeft plant
        , div [ flex, Flex.col, flex1 ]
            (viewDesc plant
                ++ [ viewPlantStat "Soil" plant.soil
                   , viewPlantStat "Regions" (String.join ", " plant.regions)
                   , viewPlantStat "Group" plant.group
                   , viewPlantStat "Age" plant.created
                   , interactionButtons False id
                   ]
            )
        ]


viewDesc : PlantModel -> List (Html msg)
viewDesc plant =
    [ div largeCentered [ text "Description" ]
    , div [] [ text plant.description ]
    , div largeCentered [ text <| formatPrice plant.price ]
    ]


viewPlantLeft : PlantModel -> Html Msg
viewPlantLeft plant =
    div [ flex, Flex.col, flex1 ]
        [ div largeCentered [ text plant.name ]
        , Html.map Images <| ImageList.view plant.images
        , viewPlantStat "Caretaker Credentials" (credsToString plant.caretakerCreds)
        , viewPlantStat "Seller Credentials" (credsToString plant.sellerCreds)
        , viewPlantStat "Seller Name" plant.sellerName
        , viewPlantStat "Seller Phone" plant.sellerPhone
        ]


credsToString : PersonCreds -> String
credsToString creds =
    String.fromInt creds.cared ++ " cared, " ++ String.fromInt creds.sold ++ " sold, " ++ String.fromInt creds.instructions ++ " instructions published"


viewPlantStat desc value =
    flexRowGap (div [ mediumMargin, mediumFont ] [ text desc ]) (div [ mediumMargin, mediumFont ] [ text value ])


flexRowGap left right =
    div [ flex, Flex.row, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ left
        , right
        ]


interactionButtons : Bool -> Int -> Html Msg
interactionButtons isOrder id =
    let
        backUrl =
            if isOrder then
                "/plant/" ++ String.fromInt id

            else
                "/search"

        orderText =
            if isOrder then
                "Confirm Order"

            else
                "Order"

        orderUrl =
            if isOrder then
                "#"

            else
                "/plant/" ++ String.fromInt id ++ "/order"

        orderOnClick =
            if isOrder then
                Button.onClick Submit

            else
                Button.attrs []
    in
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href backUrl, largeFont ] ] [ text "Back" ]
        , Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href orderUrl, largeFont ], orderOnClick ] [ text orderText ]
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
            case String.toInt plantId of
                Just plantNumber ->
                    if isOrder then
                        Plant (PlantView plantNumber <| Order <| OrderView Loading Loading None Nothing)

                    else
                        Plant (PlantView plantNumber <| JustPlant Loading)

                Nothing ->
                    NoPlant


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
