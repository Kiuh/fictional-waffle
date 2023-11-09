FROM ubuntu:bionic 

COPY ./DedicatedServerBuild/ .

ENV arguments="5000 9000"

CMD ["sh", "-c", "./build.x86_64 ${arguments}"]

# docker build . -t unityimage
# docker run -p 127.0.0.1:7878:7878/udp -p 127.0.0.1:9999:9999 -e arguments="7878 9999" -it unityimage