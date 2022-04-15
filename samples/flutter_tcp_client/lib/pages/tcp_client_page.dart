import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/material.dart';

class TcpClientPage extends StatefulWidget {
  const TcpClientPage({Key? key}) : super(key: key);

  @override
  _TcpClientPageState createState() => _TcpClientPageState();
}

class _TcpClientPageState extends State<TcpClientPage> {
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
    return Column(
        mainAxisAlignment: MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Card(
            child: Column(children: [
              ListTile(
                  trailing: _isConnected
                      ? FloatingActionButton(
                          onPressed: _disconnect,
                          tooltip: 'Disconnect',
                          child: const Icon(
                            Icons.cancel,
                            semanticLabel: "연결해제",
                          ),
                        )
                      : FloatingActionButton(
                          onPressed: _connect,
                          tooltip: 'Connect',
                          child: const Icon(Icons.bolt, semanticLabel: "연결"),
                        ),
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
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  ElevatedButton(
                      onPressed: () {
                        _send("hello");
                      },
                      child: const Text("주기적으로 전송")),
                  ElevatedButton(
                      onPressed: () {
                        _send("hello");
                      },
                      child: const Text("전송")),
                  ElevatedButton(
                      onPressed: () {
                        _send("hello");
                      },
                      child: const Text("전송")),
                  ElevatedButton(
                      onPressed: () {
                        _send("hello");
                      },
                      child: const Text("전송"))
                ],
              )
            ]),
          ),
          Card(
              child: Column(
            children: [
              const Text("로그"),
              ListView.builder(
                shrinkWrap: true,
                itemBuilder: (context, index) {
                  return Text(_logs.value[index]);
                },
                itemCount: _logs.value.length,
              ),
            ],
          ))
        ]);
  }
}
