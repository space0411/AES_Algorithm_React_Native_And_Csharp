import { StatusBar } from 'expo-status-bar';
import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import CryptoES from 'crypto-es';
import * as Device from 'expo-device';
import moment from "moment";

export default class App extends React.Component {
  constructor(props) {
    super(props);
  }

  // "Header": {
  //     "ReqCode": "sample string 1",
  //     "ReqTime": "sample string 2",
  //     "DeviceId": "sample string 3",
  //     "FunCode": "sample string 4"
  //   },
  //   "Body": {}
  generateRequestData(FunCode, Body) {
    const currentDate = moment();
    let DeviceId = Device.modelId;

    return {
      Header: {
        ReqCode: currentDate.format("YYYYMMDDHHmmssSSSSSS"),
        ReqTime: currentDate.format("YYYYMMDDHHmmss"),
        DeviceId,
        FunCode,
      },
      Body
    }
  }

  componentDidMount() {
    var key = CryptoES.enc.Utf8.parse('8080808080808080');
    var iv = CryptoES.enc.Utf8.parse('8080808080808080');
    var encryptedlogin = CryptoES.AES.encrypt(CryptoES.enc.Utf8.parse("DuyNhat"), key,
      {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoES.mode.CBC,
        padding: CryptoES.pad.Pkcs7
      });

    var encryptedpassword = CryptoES.AES.encrypt(CryptoES.enc.Utf8.parse("123456"), key,
      {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoES.mode.CBC,
        padding: CryptoES.pad.Pkcs7
      });
    console.log('object :>> ', encryptedlogin.toString());
    console.log('object :>> ', encryptedpassword.toString());
    fetch(`http://192.168.1.4:54066/api/ApiTestCryptoController/Test`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(this.generateRequestData("Login", {
        User: encryptedlogin.toString(),
        Pass: encryptedpassword.toString()
      }))
    }).then(res => res.json())
      .then(
        (result) => {
          console.log('result :>> ', result.Body);
          var de = CryptoES.AES.decrypt(result.Body, key,
            {
              keySize: 128 / 8,
              iv: iv,
              mode: CryptoES.mode.CBC,
              padding: CryptoES.pad.Pkcs7
            });
            console.log('de :>> ', de.toString(CryptoES.enc.Utf8));
        }, (error) => {
          console.log('error :>> ', error);
        }
      )
  }

  render() {
    return (
      <View style={styles.container}>
        <Text>Open up App.js to start working on your app!</Text>
        <StatusBar style="auto" />
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
  },
});
