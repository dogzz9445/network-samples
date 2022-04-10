import 'dart:async';
import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'TCP 클라이언트',
      theme: ThemeData(
        // This is the theme of your application.
        //
        // Try running your application with "flutter run". You'll see the
        // application has a blue toolbar. Then, without quitting the app, try
        // changing the primarySwatch below to Colors.green and then invoke
        // "hot reload" (press "r" in the console where you ran "flutter run",
        // or simply save your changes to "hot reload" in a Flutter IDE).
        // Notice that the counter didn't reset back to zero; the application
        // is not restarted.
        primarySwatch: Colors.blue,
      ),
      home: const MyHomePage(title: 'TCP 클라이언트'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({Key? key, required this.title}) : super(key: key);

  // This widget is the home page of your application. It is stateful, meaning
  // that it has a State object (defined below) that contains fields that affect
  // how it looks.

  // This class is the configuration for the state. It holds the values (in this
  // case the title) provided by the parent (in this case the App widget) and
  // used by the build method of the State. Fields in a Widget subclass are
  // always marked "final".

  final String title;

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  Socket? _channel;
  String? _ip;
  int? _port;
  bool _isConnected = false;
  final ValueNotifier<List<String>> _logs = ValueNotifier([]);
  //final _serverIpController = TextEditingController();

  void _connect() async {
    if (_ip == null) {
      return;
    }
    if (_port == null) {
      return;
    }
    var socket = await Socket.connect(_ip, _port ?? 5000);
    // TODO: 에러처리 제대로
    // .catchError((e) {
    //   _log(e.toString());
    // });

    setState(() {
      _isConnected = true;
      _channel = socket;
    });

    _channel?.listen((event) {
      _onReceived(socket.address, event);
    }, onDone: () {}, onError: (e) {}, cancelOnError: false);
  }

  void _disconnect() {
    setState(() {
      _isConnected = false;
    });
    _channel?.close();
  }

  void _send(String message) {
    if (message.isNotEmpty) {
      _channel?.write(message);
    }
  }

  void _onReceived(address, Uint8List event) {
    // 로그에 저장되는 내용: "<아이피>: <데이터>";
    _logs.value.add(address + String.fromCharCodes(event).trim());
  }

  void _log(String message) {
    _logs.value.add(message);
  }

  @override
  Widget build(BuildContext context) {
    // This method is rerun every time setState is called, for instance as done
    // by the _incrementCounter method above.
    //
    // The Flutter framework has been optimized to make rerunning build methods
    // fast, so that you can just rebuild anything that needs updating rather
    // than having to individually change instances of widgets.
    return Scaffold(
      appBar: AppBar(
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: Text(widget.title),
      ),
      body: Center(
          // Center is a layout widget. It takes a single child and positions it
          // in the middle of the parent.
          child: Column(
        // Column is also a layout widget. It takes a list of children and
        // arranges them vertically. By default, it sizes itself to fit its
        // children horizontally, and tries to be as tall as its parent.
        //
        // Invoke "debug painting" (press "p" in the console, choose the
        // "Toggle Debug Paint" action from the Flutter Inspector in Android
        // Studio, or the "Toggle Debug Paint" command in Visual Studio Code)
        // to see the wireframe for each widget.
        //
        // Column has various properties to control how it sizes itself and
        // how it positions its children. Here we use mainAxisAlignment to
        // center the children vertically; the main axis here is the vertical
        // axis because Columns are vertical (the cross axis would be
        // horizontal).
        mainAxisAlignment: MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Card(
            child: Column(children: [
              ListTile(
                  dense: true,
                  leading: const Text("IP"),
                  title: TextField(
                      // TODO: 아이피 Validation
                      enabled: !_isConnected,
                      decoration: const InputDecoration(
                        border: InputBorder.none,
                        hintText: '아이피 입력',
                      ),
                      onChanged: (text) {
                        setState(() {
                          _ip = text;
                        });
                      })),
              ListTile(
                dense: true,
                leading: const Text("Port"),
                title: TextField(
                    // TODO: 포트 번호 Validation
                    enabled: !_isConnected,
                    keyboardType: TextInputType.number,
                    decoration: const InputDecoration(
                      border: InputBorder.none,
                      hintText: '포트 입력',
                    ),
                    onChanged: (text) {
                      setState(() {
                        _port = int.parse(text);
                      });
                    }),
              ),
            ]),
          ),
          Card(
            child: Column(children: [
              const Text("전송 테스트"),
              ElevatedButton(
                  onPressed: () {
                    _send("hello");
                  },
                  child: const Text("전송"))
            ]),
          ),
          // Card(
          //   child: ListView.builder(
          //     itemBuilder: (context, index) {
          //       return Text(_logs.value[index]);
          //     },
          //     itemCount: _logs.value.length,
          //   ),
          // )
        ],
      )),
      // TODO: 토글로 바꾸기
      floatingActionButton: _isConnected
          ? FloatingActionButton(
              onPressed: _disconnect,
              tooltip: 'Disconnect',
              child: const Icon(Icons.cancel),
            )
          : FloatingActionButton(
              onPressed: _connect,
              tooltip: 'Connect',
              child: const Icon(Icons.bolt),
            ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
