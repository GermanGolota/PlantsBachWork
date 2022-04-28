import AES from 'crypto-js/aes';
import Utf8 from 'crypto-js/enc-utf8';

const secret = "CIPHERKEY";
const valuesKey = "PlantAuthToken";

type Roles = "Consumer" | "Producer" | "Manager";
interface AuthResponse {
  roles: Roles[];
  token: String;
}


const encrypt = (value: string) => {
  var cipher = AES.encrypt(value, secret);
  return cipher.toString();
};

const decrypt = (cipher: string) => {
  var decipher = AES.decrypt(cipher, secret);
  return decipher.toString(Utf8);
};

const store = (response: AuthResponse) => {
  let str = encrypt(JSON.stringify(response));
  localStorage.setItem(valuesKey, str);
};

const retrieve = (): AuthResponse => {
  let storedVal = localStorage.getItem(valuesKey) ?? '';
  let str = decrypt(storedVal);
  let res;
  if (str) {
    res = JSON.parse(str);
  }
  else {
    res = null;
  }
  return res;
};

export { Roles, AuthResponse, store, retrieve };