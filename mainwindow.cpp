
#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <QtCore/QDebug>
#include <QtCore/QFile>
#include <QtWebSockets/QWebSocket>
#include <QJsonObject>
#include <QJsonArray>
#include <QJsonDocument>

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::MainWindow)
{
    ui->setupUi(this);

    connect(&m_webSocket, &QWebSocket::connected, this, &MainWindow::onConnected);
    connect(&m_webSocket, QOverload<const QList<QSslError>&>::of(&QWebSocket::sslErrors), this, &MainWindow::onSslErrors);
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::on_pushButton_clicked()
{
    ui->MyLabel->setText("My text setted from button");



    m_webSocket.open(QUrl("wss://stream.binance.com:9443/ws"));
}

void MainWindow::onConnected()
{
    qDebug() << "WebSocket connected";
    connect(&m_webSocket, &QWebSocket::textMessageReceived, this, &MainWindow::onTextMessageReceived);
    QJsonObject subscribeRequestObject;
    subscribeRequestObject.insert("id", 1);
    subscribeRequestObject.insert("method", "SUBSCRIBE");
    QJsonArray params;
    params.push_back("btcusdt@trade");
    subscribeRequestObject.insert("params", params);

    m_webSocket.sendTextMessage(QJsonDocument(subscribeRequestObject).toJson());
}

void MainWindow::onTextMessageReceived(QString message)
{
    qDebug() << "Message received:" << message;
}

void MainWindow::onSslErrors(const QList<QSslError> &errors)
{
    qWarning() << "SSL errors:" << errors;

    qApp->quit();
}

