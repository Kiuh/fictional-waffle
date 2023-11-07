FROM ubuntu:bionic 

COPY ./DedicatedServerBuild/ .

ENV udp_port="5000"
ENV http_port="5000"

CMD ["sh", "-c", "./build.x86_64 ${udp_port} ${http_port}"]

# docker build . -t unityimage
# docker run -p 127.0.0.1:7878:7878/udp -p 127.0.0.1:9999:9999 -e upd_port="7878" -e http_port="9999" -it unityimage