<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:variable name="PageWidth" select="500" />
  <xsl:variable name="SeparatorLinePosition" select="'0 0 510 0'" />
  

  <xsl:template name="HeaderLine">
    <xsl:param name="Text"/>
    <Row FixedRowHeight="7" VerticalAlignment="Center">
    </Row>
    <Row FixedRowHeight="15" VerticalAlignment="Center" IsBroken="false" IsInNewPage="false">
      <Cell  BackgroundColor="LightGray" ColumnsSpan="2" Alignment="Center">
        <Text>
          <Segment>
            <xsl:value-of select="$Text"/>
          </Segment>
        </Text>
      </Cell>
    </Row>
  </xsl:template>

  <xsl:template name="HeaderLine5">
    <xsl:param name="Text"/>
    <Row FixedRowHeight="15" VerticalAlignment="Center" IsBroken="false" IsInNewPage="false" >
      <Cell  BackgroundColor="LightGray" ColumnsSpan="5" Alignment="Center">
        <Text>
          <Segment>
            <xsl:value-of select="$Text"/>
          </Segment>
        </Text>
      </Cell>
    </Row>
  </xsl:template>


  <xsl:template name="TwoColBoldRow">
    <xsl:param name="One"/>
    <xsl:param name="Two"/>

    <Row IsBroken="false">
      <Cell>
        <Text FontName="Helvetica-Bold" IsUnderline="true">
          <Segment>
            <xsl:value-of select="$One"/>
          </Segment>
        </Text>
      </Cell>
      <Cell>
        <Text FontName="Helvetica-Bold" IsUnderline="true">
          <Segment>
            <xsl:value-of select="$Two"/>
          </Segment>
        </Text>
      </Cell>
    </Row>
  </xsl:template>

  <xsl:template name="TwoColRow">
    <xsl:param name="One"/>
    <xsl:param name="Two"/>
    <Row>
      <Cell>
        <Text FontName="Helvetica">
          <Segment>
            <xsl:value-of select="$One"/>
          </Segment>
        </Text>
      </Cell>
      <Cell>
        <Text IsSpaced="true" FontName="Helvetica">
          <Segment>
            <xsl:value-of select="$Two"/>
          </Segment>
        </Text>
      </Cell>
    </Row>
  </xsl:template>



  <xsl:template name="Row">
    <xsl:param name="Obj"/>
    <xsl:param name="Head"/>
    <xsl:param name="No"/>
    <xsl:param name="Yes"/>
    <xsl:param name="List"/>
    <Row>
      <Cell Alignment="Right" VerticalAlignment="Top">
        <Text FontName="Helvetica-Bold">
          <Segment>
            <xsl:choose>
              <xsl:when test="not($Head)">
                <xsl:value-of select="$Obj/@name"/>:
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$Head"/>:
              </xsl:otherwise>
            </xsl:choose>
          </Segment>
        </Text>
      </Cell>
      <Cell>
        <Text>
          <Segment>
            <xsl:choose>
              <xsl:when test="$No and $Yes">
                <xsl:if test="$Obj/value=0">
                  <xsl:value-of select="$No"/>
                </xsl:if>
                <xsl:if test="$Obj/value=1">
                  <xsl:value-of select="$Yes"/>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$List">
                <xsl:for-each select="$Obj/value">
                  <xsl:if test="position()>1">
                    <xsl:value-of select="$List"/>
                  </xsl:if>
                  <xsl:value-of select="."/>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$Obj/value"/>
              </xsl:otherwise>
            </xsl:choose>
          </Segment>
        </Text>
      </Cell>
    </Row>
  </xsl:template>


  <xsl:template match="text()" name="lines">
    <xsl:param name="pText" select="."/>

    <xsl:if test="string-length($pText)">
      <xsl:text/>
      <xsl:value-of select="substring-before(concat($pText, '&#xA;'), '&#xA;')"/>

      <xsl:call-template name="lines">
        <xsl:with-param name="pText" select="substring-after($pText, '&#xA;')"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Source -->
  <xsl:template match="/object">


    <Pdf xmlns="Aspose.Pdf">
      <Section PageSize="letter" IsLandscape="false" PageMarginTop="45" PageMarginBottom="45" PageMarginLeft="45" PageMarginRight="45" FontName="Helvetica">
        <Header DistanceFromEdge="45">
          <Text MarginBottom="0" Alignment="Center" >
            <Segment FontSize="10">
              Insta
            </Segment>
          </Text>
          <Text MarginBottom="12" Alignment="Center" >
            <Segment FontName="Helvetica-Bold" FontSize="10">
              Document Information
            </Segment>
          </Text>
          <Text MarginBottom="3">
            <Segment FontSize="10">
              Name: <xsl:value-of select="properties/prop[@id=0]/value" />
            </Segment>
          </Text>
          <Graph Height="3" Alignment="Center">
            <xsl:attribute name="Width">
              <xsl:value-of select="$PageWidth" />
            </xsl:attribute>
            <Line Color="Black">
              <xsl:attribute name="Position">
                <xsl:value-of select="$SeparatorLinePosition" />
              </xsl:attribute>
            </Line>
          </Graph>
        </Header>

        <!--FOOTER-->
        <Footer DistanceFromEdge="30">
          <!--SEPARATOR-->
          <Graph Height="3" Alignment="Center">
            <xsl:attribute name="Width">
              <xsl:value-of select="$PageWidth" />
            </xsl:attribute>
            <Line Color="Black">
              <xsl:attribute name="Position">
                <xsl:value-of select="$SeparatorLinePosition" />
              </xsl:attribute>
            </Line>
          </Graph>

          <!--REPORT TITLE LINE 2-->
          <FloatingBox Width="250" Height="25" BoxHorizontalPositioning="Margin" BoxHorizontalAlignment="Left" BoxVerticalPositioning="Page" BoxVerticalAlignment="Bottom">
            <Text Alignment="Left" FontName="Helvetica" >
              <Segment FontSize="8">Created On:</Segment>
              <xsl:variable name="D" select="properties/prop[@id='20']/value" />
              <Segment FontSize="8" DateFormat="dd-MMM-yyyy HH:mm:ss">$D UTC</Segment>
            </Text>
          </FloatingBox>

          <!--CREATED ON TIMESTAMP-->
          <FloatingBox Width="150" Height="25" BoxHorizontalPositioning="Margin" BoxHorizontalAlignment="Center" BoxVerticalPositioning="Page" BoxVerticalAlignment="Bottom" >
            <Text Alignment="Center" FontName="Helvetica">
              <Segment FontSize="8">
                Target has no file
              </Segment>
            </Text>
          </FloatingBox>

          <!--PAGE NUMBER-->
          <FloatingBox Width="150" Height="25" BoxHorizontalPositioning="Margin" BoxHorizontalAlignment="Right" BoxVerticalPositioning="Page" BoxVerticalAlignment="Bottom" >
            <Text Alignment="Right" FontName="Helvetica">
              <Segment FontSize="8">Page $p of $P</Segment>
            </Text>
          </FloatingBox>
        </Footer>

        <!-- INITIATOR-->
        <Table MarginTop="15" ColumnWidths="150 360" DefaultCellPaddingTop="2" DefaultCellPaddingLeft="3"
             DefaultCellPaddingRight="3" DefaultCellPaddingBottom="2" IsFirstRowRepeated="True" FontSize="10"
             VerticalAlignment="Top" Alignment="Left">
          <DefaultCellBorder>
            <All LineWidth="0"></All>
          </DefaultCellBorder>
          <Border>
            <All LineWidth="0"></All>
          </Border>
          <Row FixedRowHeight="1">
            <Cell/>
            <Cell/>
          </Row>

          <!-- ASSOCIATED DOCUMENT HEADER-->
          <xsl:call-template name="HeaderLine">
            <xsl:with-param name="Text">Target has no file</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="TwoColRow">
            <xsl:with-param name="Two">This target has no file or it was not converted to PDF.</xsl:with-param>
          </xsl:call-template>

        </Table>


      </Section>
    </Pdf>
  </xsl:template>
</xsl:stylesheet>