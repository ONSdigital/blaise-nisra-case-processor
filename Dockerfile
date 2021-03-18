FROM mono:6.12.0

COPY Blaise.Case.Nisra.Processor.WindowsService\bin\Debug /nisra-service

WORKDIR /nisra-service

ENTRYPOINT ["mono", "Blaise.Case.Nisra.Processor.exe"]